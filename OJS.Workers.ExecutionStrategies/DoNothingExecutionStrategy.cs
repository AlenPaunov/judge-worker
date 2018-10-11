﻿namespace OJS.Workers.ExecutionStrategies
{
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

    public class DoNothingExecutionStrategy : IExecutionStrategy
    {
        protected string WorkingDirectory { get; set; }

        public IExecutionResult<TResult> SafeExecute<TResult>(IExecutionContext executionContext)
            where TResult : ISingleCodeRunResult, new()
        {
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();

            try
            {
                return this.Execute<TResult>(executionContext);
            }
            finally
            {
                DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
            }
        }

        public IExecutionResult<TResult> Execute<TResult>(IExecutionContext executionContext)
            where TResult : ISingleCodeRunResult, new() =>
                new ExecutionResult<TResult>
                {
                    CompilerComment = null,
                    IsCompiledSuccessfully = true
                };
    }
}
