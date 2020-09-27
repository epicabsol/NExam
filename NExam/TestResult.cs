using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NExam
{
    /// <summary>
    /// The result of a test that has been run.
    /// </summary>
    public sealed class TestResult
    {
        /// <summary>
        /// The method that was tested.
        /// </summary>
        public MethodInfo TestMethod { get; }

        /// <summary>
        /// Whether the test failed.
        /// </summary>
        public bool Failed { get; }

        /// <summary>
        /// The messages that were emitted during the test's run.
        /// </summary>
        public IReadOnlyList<string> Messages { get; }

        /// <summary>
        /// Creates a new TestResult with the given results.
        /// </summary>
        /// <param name="testMethod">The method that was tested.</param>
        /// <param name="failed">Whether the test failed.</param>
        /// <param name="messages">The messages that were emitted during the test's run.</param>
        public TestResult(MethodInfo testMethod, bool failed, IEnumerable<string> messages)
        {
            this.TestMethod = testMethod;
            this.Failed = failed;
            this.Messages = new List<string>(messages).AsReadOnly();
        }
    }
}
