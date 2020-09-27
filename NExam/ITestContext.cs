using System;
using System.Collections.Generic;
using System.Text;

namespace NExam
{
    /// <summary>
    /// The context that test methods run within. Test methods use the given context to interact with the test run.
    /// </summary>
    public interface ITestContext
    {
        void Assert(bool condition, string failureMessage);
        void Log(string message);
        void RunExpectingException<T>(Action actionToRun, string failureMessage) where T : Exception;
    }
}
