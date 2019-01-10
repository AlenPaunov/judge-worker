﻿namespace OJS.Workers.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;
    using OJS.Workers.Common;

    // TODO: Implement memory constraints
    public class StandardProcessExecutor : IExecutor
    {
        private static ILog logger;

        private readonly int baseTimeUsed;
        private readonly int baseMemoryUsed;
        private readonly ITasksService tasksService;

        public StandardProcessExecutor() =>
            logger = LogManager.GetLogger(typeof(StandardProcessExecutor));

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardProcessExecutor"/> class with base time and memory used.
        /// </summary>
        /// <param name="baseTimeUsed">The base time in milliseconds added to the time limit when executing.</param>
        /// <param name="baseMemoryUsed">The base memory in bytes added to the memory limit when executing.</param>
        public StandardProcessExecutor(int baseTimeUsed, int baseMemoryUsed, ITasksService tasksService)
            : this()
        {
            this.baseTimeUsed = baseTimeUsed;
            this.baseMemoryUsed = baseMemoryUsed;
            this.tasksService = tasksService;
        }

        public ProcessExecutionResult Execute(
            string fileName,
            string inputData,
            int timeLimit,
            int memoryLimit,
            IEnumerable<string> executionArguments = null,
            string workingDirectory = null,
            bool useProcessTime = false,
            bool useSystemEncoding = false,
            bool dependOnExitCodeForRunTimeError = false,
            double timeoutMultiplier = 1.5)
        {
            timeLimit = timeLimit + this.baseTimeUsed;
            memoryLimit = memoryLimit + this.baseMemoryUsed;

            var result = new ProcessExecutionResult { Type = ProcessExecutionResultType.Success };
            if (workingDirectory == null)
            {
                workingDirectory = new FileInfo(fileName).DirectoryName;
            }

            var processStartInfo = new ProcessStartInfo(fileName)
            {
                Arguments = executionArguments == null ? string.Empty : string.Join(" ", executionArguments),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                ErrorDialog = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WorkingDirectory = workingDirectory,
                StandardOutputEncoding = useSystemEncoding ? Encoding.Default : new UTF8Encoding(false)
            };

            using (var process = System.Diagnostics.Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new Exception($"Could not start process: {fileName}!");
                }

                var startTime = process.StartTime;
                #if !DEBUG
                process.PriorityClass = ProcessPriorityClass.High;
                #endif
                // Write to standard input using another thread
                process.StandardInput.WriteLineAsync(inputData).ContinueWith(
                    _ => process.StandardInput.FlushAsync().ContinueWith(
                        __ => process.StandardInput.Close()));

                // Read standard output using another thread to prevent process locking (waiting us to empty the output buffer)
                var processOutputTask = process.StandardOutput.ReadToEndAsync().ContinueWith(
                    x => { result.ReceivedOutput = x.Result; });

                // Read standard error using another thread
                var errorOutputTask = process.StandardError.ReadToEndAsync().ContinueWith(
                    x => { result.ErrorOutput = x.Result; });

                // Read memory consumption every few milliseconds to determine the peak memory usage of the process
                const int memoryIntervalBetweenTwoMemoryConsumptionRequests = 45;
                var memoryRunInBackgroundInfo = this.tasksService.RunWithInterval(
                    memoryIntervalBetweenTwoMemoryConsumptionRequests,
                    () =>
                    {
                        if (process.HasExited)
                        {
                            return;
                        }

                        var peakWorkingSetSize = process.PeakWorkingSet64;
                        Console.WriteLine(DateTime.Now + ": " + peakWorkingSetSize);

                        result.MemoryUsed = Math.Max(result.MemoryUsed, peakWorkingSetSize);
                    });

                const int timeIntervalBetweenTwoTimeConsumptionRequests = 10;
                var timeRunInBackgroundInfo = this.tasksService.RunWithInterval(
                    timeIntervalBetweenTwoTimeConsumptionRequests,
                    () =>
                    {
                        if (process.HasExited)
                        {
                            return;
                        }

                        result.PrivilegedProcessorTime = process.PrivilegedProcessorTime;
                        result.UserProcessorTime = process.UserProcessorTime;
                    });
                // Read time consumption every few milliseconds to determine the peak memory usage of the process

                // Wait the process to complete. Kill it after (timeLimit * 1.5) milliseconds if not completed.
                // We are waiting the process for more than defined time and after this we compare the process time with the real time limit.
                var exited = process.WaitForExit((int)(timeLimit * timeoutMultiplier));
                if (!exited)
                {
                    // Double check if the process has exited before killing it
                    if (!process.HasExited)
                    {
                        process.Kill();

                        // Approach: https://msdn.microsoft.com/en-us/library/system.diagnostics.process.kill(v=vs.110).aspx#Anchor_2
                        process.WaitForExit(Constants.DefaultProcessExitTimeOutMilliseconds);
                    }

                    result.Type = ProcessExecutionResultType.TimeLimit;
                }

                try
                {
                    // To be sure that memory consumption will be evaluated correctly
                    this.tasksService.Stop(memoryRunInBackgroundInfo);
                }
                catch (AggregateException ex)
                {
                    logger.Warn("AggregateException caught.", ex.InnerException);
                }

                try
                {
                    // To be sure that memory consumption will be evaluated correctly
                    this.tasksService.Stop(timeRunInBackgroundInfo);
                }
                catch (AggregateException ex)
                {
                    logger.Warn("AggregateException caught.", ex.InnerException);
                }

                // Close the time check thread

                // Close the task that gets the process error output
                try
                {
                    errorOutputTask.Wait(100);
                }
                catch (AggregateException ex)
                {
                    logger.Warn("AggregateException caught.", ex.InnerException);
                }

                // Close the task that gets the process output
                try
                {
                    processOutputTask.Wait(100);
                }
                catch (AggregateException ex)
                {
                    logger.Warn("AggregateException caught.", ex.InnerException);
                }

                Debug.Assert(process.HasExited, "Standard process didn't exit!");

                // Report exit code and total process working time
                result.ExitCode = process.ExitCode;
                result.TimeWorked = process.ExitTime - startTime;
                result.ReceivedOutput += result.PrivilegedProcessorTime;
                result.ReceivedOutput += "      ";
                result.ReceivedOutput += result.UserProcessorTime;
                result.ReceivedOutput += "      ";
            }

            if ((useProcessTime && result.TimeWorked.TotalMilliseconds > timeLimit) ||
                result.TotalProcessorTime.TotalMilliseconds > timeLimit)
            {
                result.Type = ProcessExecutionResultType.TimeLimit;
            }

            if (result.MemoryUsed > memoryLimit)
            {
                result.Type = ProcessExecutionResultType.MemoryLimit;
            }

            if (!string.IsNullOrEmpty(result.ErrorOutput) ||
                (dependOnExitCodeForRunTimeError && result.ExitCode < -1))
            {
                result.Type = ProcessExecutionResultType.RunTimeError;
            }

            result.ApplyTimeAndMemoryOffset(this.baseTimeUsed, this.baseMemoryUsed);

            return result;
        }
    }
}
