﻿namespace OJS.Workers.ExecutionStrategies
{
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies.Models;

    public class CheckOnlyExecutionStrategy : ExecutionStrategy
    {
        public CheckOnlyExecutionStrategy(int baseTimeUsed, int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
        }

        protected override IExecutionResult<TestResult> ExecuteCompetitive(
            CompetitiveExecutionContext executionContext)
        {
            var result = new ExecutionResult<TestResult>
            {
                IsCompiledSuccessfully = true
            };

            var processExecutionResult = new ProcessExecutionResult
            {
                Type = ProcessExecutionResultType.Success,
                ReceivedOutput = executionContext.Code
            };

            var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            foreach (var test in executionContext.Tests)
            {
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.Results.Add(testResult);
            }

            return result;
        }
    }
}