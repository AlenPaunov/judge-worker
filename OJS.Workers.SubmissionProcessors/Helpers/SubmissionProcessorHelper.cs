﻿namespace OJS.Workers.SubmissionProcessors.Helpers
{
    using System;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.ExecutionStrategies.Blockchain;
    using OJS.Workers.ExecutionStrategies.CPlusPlus;
    using OJS.Workers.ExecutionStrategies.CSharp;
    using OJS.Workers.ExecutionStrategies.CSharp.DotNetCore.v3;
    using OJS.Workers.ExecutionStrategies.CSharp.DotNetCore.v6;
    using OJS.Workers.ExecutionStrategies.Java;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.ExecutionStrategies.NodeJs;
    using OJS.Workers.ExecutionStrategies.PHP;
    using OJS.Workers.ExecutionStrategies.Python;
    using OJS.Workers.ExecutionStrategies.Ruby;
    using OJS.Workers.ExecutionStrategies.Sql.MySql;
    using OJS.Workers.ExecutionStrategies.Sql.SqlServerLocalDb;
    using OJS.Workers.ExecutionStrategies.Sql.SqlServerSingleDatabase;
    using OJS.Workers.Executors.Implementations;
    using OJS.Workers.SubmissionProcessors.Models;

    public static class SubmissionProcessorHelper
    {
        public static IExecutionStrategy CreateExecutionStrategy(ExecutionStrategyType type, int portNumber)
        {
            IExecutionStrategy executionStrategy;
            var tasksService = new TasksService();
            var processExecutorFactory = new ProcessExecutorFactory(tasksService);
            var submissionProcessorIdentifier = portNumber.ToString();
            switch (type)
            {
                case ExecutionStrategyType.CompileExecuteAndCheck:
                    executionStrategy = new CompileExecuteAndCheckExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy:
                    executionStrategy = new CPlusPlusCompileExecuteAndCheckExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.GPlusPlusBaseTimeUsedInMilliseconds,
                        Settings.GPlusPlusBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy:
                    executionStrategy = new CPlusPlusZipFileExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.GPlusPlusBaseTimeUsedInMilliseconds,
                        Settings.GPlusPlusBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck:
                    executionStrategy = new DotNetCoreCompileExecuteAndCheckExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.DotNetCoreRuntimeVersion,
                        Settings.DotNetCscBaseTimeUsedInMilliseconds,
                        Settings.DotNetCscBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreTestRunner:
                    executionStrategy = new DotNetCoreTestRunnerExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreUnitTestsExecutionStrategy:
                    executionStrategy = new DotNetCoreUnitTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpUnitTestsExecutionStrategy:
                    executionStrategy = new CSharpUnitTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.NUnitConsoleRunnerPath,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpProjectTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.NUnitConsoleRunnerPath,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpPerformanceProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpPerformanceProjectTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.NUnitConsoleRunnerPath,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpAspProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpAspProjectTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.NUnitConsoleRunnerPath,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreProjectExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCore6ProjectTestsExecutionStrategy:
                    executionStrategy = new DotNetCore6ProjectTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.RubyExecutionStrategy:
                    executionStrategy = new RubyExecutionStrategy(
                        processExecutorFactory,
                        Settings.RubyPath,
                        Settings.RubyBaseTimeUsedInMilliseconds,
                        Settings.RubyBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck:
                    executionStrategy = new JavaPreprocessCompileExecuteAndCheckExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.JavaExecutablePath,
                        Settings.JavaLibsPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes,
                        Settings.JavaBaseUpdateTimeOffsetInMilliseconds);
                    break;
                case ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck:
                    executionStrategy = new JavaZipFileCompileExecuteAndCheckExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.JavaExecutablePath,
                        Settings.JavaLibsPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaProjectTestsExecutionStrategy:
                    executionStrategy = new JavaProjectTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.JavaExecutablePath,
                        Settings.JavaLibsPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaUnitTestsExecutionStrategy:
                    executionStrategy = new JavaUnitTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.JavaExecutablePath,
                        Settings.JavaLibsPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy:
                    executionStrategy = new JavaSpringAndHibernateProjectExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.JavaExecutablePath,
                        Settings.JavaLibsPath,
                        Settings.MavenPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                    executionStrategy = new NodeJsPreprocessExecuteAndCheckExecutionStrategy(
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds * 2,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha:
                    executionStrategy = new NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMochaExecutionStrategy(
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BrowserifyModulePath,
                        Settings.BabelifyModulePath,
                        Settings.Es2015ImportPluginPath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy(
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy:
                    executionStrategy = new NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy(
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonJsDomModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BabelCoreModulePath,
                        Settings.ReactJsxPluginPath,
                        Settings.ReactModulePath,
                        Settings.ReactDomModulePath,
                        Settings.NodeFetchModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy:
                    executionStrategy = new NodeJsZipExecuteHtmlAndCssStrategy(
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BootstrapModulePath,
                        Settings.BootstrapCssPath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategy:
                    executionStrategy = new RunSpaAndExecuteMochaTestsExecutionStrategy(
                        processExecutorFactory,
                        Settings.PythonExecutablePath,
                        Settings.JsProjNodeModules,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.PlaywrightModulePath,
                        portNumber,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PythonExecuteAndCheck:
                    executionStrategy = new PythonExecuteAndCheckExecutionStrategy(
                        processExecutorFactory,
                        Settings.PythonExecutablePath,
                        Settings.PythonBaseTimeUsedInMilliseconds,
                        Settings.PythonBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PythonUnitTests:
                    executionStrategy = new PythonUnitTestsExecutionStrategy(
                        processExecutorFactory,
                        Settings.PythonExecutablePath,
                        Settings.PythonBaseTimeUsedInMilliseconds,
                        Settings.PythonBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PythonCodeExecuteAgainstUnitTests:
                    executionStrategy = new PythonCodeExecuteAgainstUnitTestsExecutionStrategy(
                        processExecutorFactory,
                        Settings.PythonExecutablePath,
                        Settings.PythonBaseTimeUsedInMilliseconds,
                        Settings.PythonBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PythonProjectTests:
                    executionStrategy = new PythonProjectTestsExecutionStrategy(
                        processExecutorFactory,
                        Settings.PythonExecutablePath,
                        Settings.PythonBaseTimeUsedInMilliseconds,
                        Settings.PythonBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PythonProjectUnitTests:
                    executionStrategy = new PythonProjectUnitTestsExecutionStrategy(
                        processExecutorFactory,
                        Settings.PythonExecutablePath,
                        Settings.PythonBaseTimeUsedInMilliseconds,
                        Settings.PythonBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpProjectExecutionStrategy:
                    executionStrategy = new PhpProjectExecutionStrategy(
                        processExecutorFactory,
                        Settings.PhpCliExecutablePath,
                        Settings.PhpCliBaseTimeUsedInMilliseconds,
                        Settings.PhpCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpProjectWithDbExecutionStrategy:
                    executionStrategy = new PhpProjectWithDbExecutionStrategy(
                        processExecutorFactory,
                        Settings.PhpCliExecutablePath,
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword,
                        Settings.PhpCliBaseTimeUsedInMilliseconds,
                        Settings.PhpCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpCgiExecuteAndCheck:
                    executionStrategy = new PhpCgiExecuteAndCheckExecutionStrategy(
                        processExecutorFactory,
                        Settings.PhpCgiExecutablePath,
                        Settings.PhpCgiBaseTimeUsedInMilliseconds,
                        Settings.PhpCgiBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpCliExecuteAndCheck:
                    executionStrategy = new PhpCliExecuteAndCheckExecutionStrategy(
                        processExecutorFactory,
                        Settings.PhpCliExecutablePath,
                        Settings.PhpCliBaseTimeUsedInMilliseconds,
                        Settings.PhpCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.SolidityCompileDeployAndRunUnitTestsExecutionStrategy:
                    executionStrategy = new SolidityCompileDeployAndRunUnitTestsExecutionStrategy(
                        GetCompilerPath,
                        processExecutorFactory,
                        Settings.NodeJsExecutablePath,
                        Settings.GanacheCliNodeExecutablePath,
                        Settings.TruffleCliNodeExecutablePath,
                        portNumber,
                        Settings.SolidityBaseTimeUsedInMilliseconds,
                        Settings.SolidityBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbPrepareDatabaseAndRunQueries:
                    executionStrategy = new SqlServerLocalDbPrepareDatabaseAndRunQueriesExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerLocalDbRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabasePrepareDatabaseAndRunQueries:
                    executionStrategy = new SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword,
                        submissionProcessorIdentifier);
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabaseRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword,
                        submissionProcessorIdentifier);
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword,
                        submissionProcessorIdentifier);
                    break;
                case ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries:
                    executionStrategy = new MySqlPrepareDatabaseAndRunQueriesExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.DoNothing:
                    executionStrategy = new DoNothingExecutionStrategy();
                    break;
                case ExecutionStrategyType.RemoteExecution:
                    executionStrategy = new RemoteExecutionStrategy();
                    break;
                case ExecutionStrategyType.CheckOnly:
                    executionStrategy = new CheckOnlyExecutionStrategy(processExecutorFactory, 0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return executionStrategy;
        }

        public static IExecutionContext<TInput> CreateExecutionContext<TInput>(
            OjsSubmission<TInput> submission)
        {
            if (submission == null)
            {
                throw new ArgumentNullException(nameof(submission));
            }

            return new ExecutionContext<TInput>
            {
                AdditionalCompilerArguments = submission.AdditionalCompilerArguments,
                Code = submission.Code,
                FileContent = submission.FileContent,
                AllowedFileExtensions = submission.AllowedFileExtensions,
                CompilerType = submission.CompilerType,
                MemoryLimit = submission.MemoryLimit,
                TimeLimit = submission.TimeLimit,
                Input = submission.Input
            };
        }

        private static string GetCompilerPath(CompilerType type)
        {
            switch (type)
            {
                case CompilerType.None:
                    return null;
                case CompilerType.CSharp:
                    return Settings.CSharpCompilerPath;
                case CompilerType.MsBuild:
                case CompilerType.MsBuildLibrary:
                    return Settings.MsBuildExecutablePath;
                case CompilerType.CPlusPlusGcc:
                case CompilerType.CPlusPlusZip:
                    return Settings.CPlusPlusGccCompilerPath;
                case CompilerType.Java:
                case CompilerType.JavaZip:
                case CompilerType.JavaInPlaceCompiler:
                    return Settings.JavaCompilerPath;
                case CompilerType.DotNetCompiler:
                case CompilerType.CSharpDotNetCore:
                    return Settings.DotNetCompilerPath;
                case CompilerType.SolidityCompiler:
                    return Settings.SolidityCompilerPath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
