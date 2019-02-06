﻿namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class CPlusPlusZipFileExecutionStrategy : BaseCodeExecutionStrategy
    {
        private const string SubmissionName = "UserSubmission.zip";
        private const string FileNameAndExtensionPattern = @"//((\w+)\.(cpp|h))//";

        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CPlusPlusZipFileExecutionStrategy(
            Func<CompilerType, string> getCompilerPath,
            IProcessExecutorFactory processExecutorFactory,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(processExecutorFactory, baseTimeUsed, baseMemoryUsed) =>
                this.getCompilerPathFunc = getCompilerPath;

        protected override void ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            var submissionDestination = $@"{this.WorkingDirectory}\{SubmissionName}";

            File.WriteAllBytes(submissionDestination, executionContext.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionDestination, RemoveMacFolderPattern);

            if (!string.IsNullOrEmpty(executionContext.Input.TaskSkeletonAsString))
            {
                var pathsOfHeadersAndCppFiles = this.ExtractTaskSkeleton(executionContext.Input.TaskSkeletonAsString);
                FileHelpers.AddFilesToZipArchive(submissionDestination, string.Empty, pathsOfHeadersAndCppFiles.ToArray());
            }

            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);

            var compilationResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                submissionDestination);

            if (!compilationResult.IsCompiledSuccessfully)
            {
                return;
            }

            result.IsCompiledSuccessfully = true;

            var executor = this.CreateExecutor(ProcessExecutorType.Restricted);

            var checker = executionContext.Input.GetChecker();

            foreach (var test in executionContext.Input.Tests)
            {
                var processExecutionResult = executor.Execute(
                    compilationResult.OutputFile,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    executionArguments: null,
                    workingDirectory: null,
                    useProcessTime: false,
                    useSystemEncoding: false,
                    dependOnExitCodeForRunTimeError: true);

                var testResults = this.CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.Results.Add(testResults);
            }
        }

        private IEnumerable<string> ExtractTaskSkeleton(string executionContextTaskSkeletonAsString)
        {
            var headersAndCppFiles = executionContextTaskSkeletonAsString.Split(
                new string[] { Constants.ClassDelimiter },
                StringSplitOptions.RemoveEmptyEntries);

            var pathsToHeadersAndCppFiles = new List<string>();
            var fileNameAndExtensionMatcher = new Regex(FileNameAndExtensionPattern);

            foreach (var headersOrCppFile in headersAndCppFiles)
            {
                var match = fileNameAndExtensionMatcher.Match(headersOrCppFile);
                if (match.Success)
                {
                    File.WriteAllText($@"{this.WorkingDirectory}\{match.Groups[1]}", headersOrCppFile);
                    pathsToHeadersAndCppFiles.Add($@"{this.WorkingDirectory}\{match.Groups[1]}");
                }
            }

            return pathsToHeadersAndCppFiles;
        }
    }
}
