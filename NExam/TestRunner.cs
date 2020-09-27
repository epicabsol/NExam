using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NExam
{
    public static class TestRunner
    {
        private class TestContext : ITestContext
        {
            private MethodInfo Method { get; }
            private bool Failed = false;
            private List<string> Messages = new List<string>();

            public TestContext(MethodInfo method)
            {
                this.Method = method;
            }

            public void Assert(bool condition, string failureMessage)
            {
                if (condition == false)
                {
                    Failed = true;
                    this.Log(failureMessage);
                }
            }

            public void Log(string message)
            {
                Messages.Add(message);
            }

            public TestResult GetResults()
            {
                return new TestResult(Method, Failed, Messages);
            }

            public void RunExpectingException<T>(Action actionToRun, string failureMessage) where T : Exception
            {
                bool exceptionGenerated = false;
                try
                {
                    actionToRun();
                }
                catch (T expected)
                {
                    exceptionGenerated = true;
                    this.Log($"Caught expected exception: {expected.GetType()}");
                }

                this.Assert(exceptionGenerated, $"Expected exception of type {typeof(T)}, but none were caught. Message: {failureMessage}");
            }
        }

        /// <summary>
        /// Runs all the unit tests in the given assembly.
        /// </summary>
        /// <param name="containingAssembly">The assembly containing the tests to run.</param>
        public static TestResult[] RunAllTests(Assembly containingAssembly)
        {
            List<TestResult> results = new List<TestResult>();
            foreach (Type type in containingAssembly.DefinedTypes)
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.IsStatic && method.GetCustomAttribute<TestAttribute>() is TestAttribute testAttribute)
                    {
                        results.Add(TestRunner.RunTest(method));
                    }
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// Runs all the tests in the calling assembly.
        /// </summary>
        public static TestResult[] RunAllTests()
        {
            return TestRunner.RunAllTests(Assembly.GetCallingAssembly());
        }

        public static void PrintResults(TextWriter writer, TestResult[] results)
        {
            int successCount = results.Count(result => !result.Failed);

            writer.WriteLine($"Test results: {successCount}/{results.Length} passed.");
            foreach (TestResult result in results)
            {
                writer.Write(result.Failed ? "[!!!] " : "[ * ] ");
                writer.WriteLine($"{result.TestMethod.DeclaringType}.{result.TestMethod.Name}");
                foreach (string message in result.Messages)
                {
                    writer.WriteLine($"         {message}");
                }
            }
        }

        /// <summary>
        /// Runs the given test.
        /// </summary>
        /// <param name="testMethod">The method which runs the test.</param>
        /// <returns>The results of the test run.</returns>
        public static TestResult RunTest(MethodInfo testMethod)
        {
            TestContext context = new TestContext(testMethod);

            try
            {
                testMethod.Invoke(null, new object[] { context });
            }
            catch (Exception ex)
            {
                context.Assert(false, "Unhandled exception during test: " + ex.ToString());
            }

            return context.GetResults();
        }
    }
}
