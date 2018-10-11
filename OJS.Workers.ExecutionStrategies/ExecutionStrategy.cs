﻿namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using log4net;

    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Models;

    public abstract class ExecutionStrategy : IExecutionStrategy
    {
        protected const string RemoveMacFolderPattern = "__MACOSX/*";

        private readonly ILog logger;

        protected ExecutionStrategy(int baseTimeUsed, int baseMemoryUsed)
        {
            this.BaseTimeUsed = baseTimeUsed;
            this.BaseMemoryUsed = baseMemoryUsed;
            this.logger = LogManager.GetLogger(Constants.LocalWorkerServiceLogName);
        }

        protected int BaseTimeUsed { get; }

        protected int BaseMemoryUsed { get; }

        protected string WorkingDirectory { get; set; }

        public ExecutionResult Execute(IExecutionContext executionContext)
        {
            switch (executionContext)
            {
                case CompetitiveExecutionContext competitiveExecutionContext:
                    return this.ExecuteCompetitive(competitiveExecutionContext);
                case NonCompetitiveExecutionContext nonCompetitiveExecutionContext:
                    return this.ExecuteNonCompetitive(nonCompetitiveExecutionContext);
                default:
                    return new ExecutionResult
                    {
                        IsCompiledSuccessfully = false,
                        CompilerComment = "Execution context not found"
                    };
            }
        }

        public ExecutionResult SafeExecute(IExecutionContext executionContext)
        {
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();
            try
            {
                return this.Execute(executionContext);
            }
            finally
            {
                Task.Run(() =>
                {
                    try
                    {
                        DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error("executionStrategy.SafeDeleteDirectory has thrown an exception:", ex);
                    }
                });
            }
        }

        protected virtual ExecutionResult ExecuteNonCompetitive(NonCompetitiveExecutionContext executionContext) =>
            throw new NotImplementedException();

        protected abstract ExecutionResult ExecuteCompetitive(CompetitiveExecutionContext executionContext);

        protected ExecutionResult CompileExecuteAndCheck(
            CompetitiveExecutionContext executionContext,
            Func<CompilerType, string> getCompilerPathFunc,
            IExecutor executor,
            bool useSystemEncoding = true,
            bool dependOnExitCodeForRunTimeError = false)
        {
            var result = new ExecutionResult();

            // Compile the file
            var compilerResult = this.ExecuteCompiling(executionContext, getCompilerPathFunc, result);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var outputFile = compilerResult.OutputFile;

            // Execute and check each test
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    outputFile,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    null,
                    null,
                    false,
                    useSystemEncoding,
                    dependOnExitCodeForRunTimeError);

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected TestResult ExecuteAndCheckTest(
            TestContext test,
            ProcessExecutionResult processExecutionResult,
            IChecker checker,
            string receivedOutput)
        {
            var testResult = new TestResult
            {
                Id = test.Id,
                TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                MemoryUsed = (int)processExecutionResult.MemoryUsed,
            };

            if (processExecutionResult.Type == ProcessExecutionResultType.RunTimeError)
            {
                testResult.ResultType = TestRunResultType.RunTimeError;
                testResult.ExecutionComment = processExecutionResult.ErrorOutput.MaxLength(2048); // Trimming long error texts
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit)
            {
                testResult.ResultType = TestRunResultType.TimeLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.MemoryLimit)
            {
                testResult.ResultType = TestRunResultType.MemoryLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.Success)
            {
                var checkerResult = checker.Check(test.Input, receivedOutput, test.Output, test.IsTrialTest);
                testResult.ResultType = checkerResult.IsCorrect ? TestRunResultType.CorrectAnswer : TestRunResultType.WrongAnswer;

                // TODO: Do something with checkerResult.ResultType
                testResult.CheckerDetails = checkerResult.CheckerDetails;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(processExecutionResult), "Invalid ProcessExecutionResultType value.");
            }

            return testResult;
        }

        protected RawResult GetRawResult(ProcessExecutionResult processExecutionResult, string receivedOutput) =>
            new RawResult
            {
                TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                MemoryUsed = (int)processExecutionResult.MemoryUsed,
                ResultType = processExecutionResult.Type,
                Output = receivedOutput
            };

        protected CompileResult ExecuteCompiling(IExecutionContext executionContext, Func<CompilerType, string> getCompilerPathFunc, ExecutionResult result)
        {
            var submissionFilePath = string.IsNullOrEmpty(executionContext.AllowedFileExtensions)
                ? FileHelpers.SaveStringToTempFile(this.WorkingDirectory, executionContext.Code)
                : FileHelpers.SaveByteArrayToTempFile(this.WorkingDirectory, executionContext.FileContent);

            var compilerPath = getCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(executionContext.CompilerType, compilerPath, executionContext.AdditionalCompilerArguments, submissionFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            return compilerResult;
        }

        protected virtual CompileResult Compile(
            CompilerType compilerType,
            string compilerPath,
            string compilerArguments,
            string submissionFilePath)
        {
            if (compilerType == CompilerType.None)
            {
                return new CompileResult(true, null) { OutputFile = submissionFilePath };
            }

            if (!File.Exists(compilerPath))
            {
                throw new ArgumentException($"Compiler not found in: {compilerPath}", nameof(compilerPath));
            }

            var compiler = Compiler.CreateCompiler(compilerType);
            var compilerResult = compiler.Compile(compilerPath, submissionFilePath, compilerArguments);

            return compilerResult;
        }
    }
}