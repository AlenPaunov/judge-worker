﻿namespace OJS.Workers.ExecutionStrategies.Helpers
{
    using System;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;

    internal static class UnitTestStrategiesHelper
    {
        public const string TestedCodeFileName = "TestedCode";

        public static readonly string TestedCodeFileNameWithExtension =
            $"{TestedCodeFileName}{Constants.CSharpFileExtension}";

        /// <summary>
        /// Gets the output message and the count of the original tests passed,
        /// by running the provided regex on the received output from the execution process
        /// </summary>
        /// <param name="receivedOutput">The output from the console runner</param>
        /// <param name="regex">The Regex used to catch the passing and failing tests</param>
        /// <param name="originalTestsPassed">The number of unit tests that have passed on the first test</param>
        /// <param name="isFirstTest">Bool indicating if the results are for the first test</param>
        /// <param name="testsCountExtractor">The function which extracts total and passed tests count from the MatchCollection</param>
        /// <returns></returns>
        public static (string message, int originalTestsPassed) GetTestResult(
            string receivedOutput,
            Regex regex,
            int originalTestsPassed,
            bool isFirstTest,
            Func<MatchCollection, (int totalTests, int passedTests)> testsCountExtractor)
        {
            var matches = regex.Matches(receivedOutput);
            if (matches.Count == 0)
            {
                throw new ArgumentException("The process did not produce any output!");
            }

            var (totalTests, passedTests) = testsCountExtractor(matches);

            var message = "Test Passed!";

            if (totalTests == 0)
            {
                message = "No tests found";
            }
            else if (passedTests >= originalTestsPassed)
            {
                message = "No functionality covering this test!";
            }

            if (isFirstTest)
            {
                originalTestsPassed = passedTests;

                if (totalTests != passedTests)
                {
                    message = "Not all tests passed on the correct solution.";
                }
                else
                {
                    message = "Test Passed!";
                }
            }

            return (message, originalTestsPassed);
        }
    }
}
