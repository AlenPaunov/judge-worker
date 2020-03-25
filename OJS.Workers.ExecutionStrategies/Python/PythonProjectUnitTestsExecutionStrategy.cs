namespace OJS.Workers.ExecutionStrategies.Python
{
    using OJS.Workers.Executors;

    public class PythonProjectUnitTestsExecutionStrategy : PythonUnitTestsExecutionStrategy
    {
        public PythonProjectUnitTestsExecutionStrategy(
            IProcessExecutorFactory processExecutorFactory,
            string pythonExecutablePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(processExecutorFactory, pythonExecutablePath, baseTimeUsed, baseMemoryUsed)
        {
        }
    }
}