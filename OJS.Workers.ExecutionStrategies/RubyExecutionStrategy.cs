﻿namespace OJS.Workers.ExecutionStrategies
{
    using System.IO;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class RubyExecutionStrategy : ExecutionStrategy
    {
        public RubyExecutionStrategy(
            IProcessExecutorFactory processExecutorFactory,
            string rubyPath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(processExecutorFactory, baseTimeUsed, baseMemoryUsed) =>
                this.RubyPath = rubyPath;

        public string RubyPath { get; set; }

        protected override IExecutionResult<TestResult> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext)
        {
            var result = new ExecutionResult<TestResult>();

            result.IsCompiledSuccessfully = true;

            var submissionFilePath = FileHelpers.SaveStringToTempFile(this.WorkingDirectory, executionContext.Code);

            var arguments = new[] { submissionFilePath };

            var executor = this.CreateExecutor(ProcessExecutorType.Restricted);

            var checker = executionContext.Input.GetChecker();

            foreach (var test in executionContext.Input.Tests)
            {
                var processExecutionResult = executor.Execute(
                    this.RubyPath,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments);

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.Results.Add(testResult);
            }

            if (File.Exists(submissionFilePath))
            {
                File.Delete(submissionFilePath);
            }

            return result;
        }
    }
}
