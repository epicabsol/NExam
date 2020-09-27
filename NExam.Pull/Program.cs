using System;
using NExam.NSubmarine;

namespace NExam.Pull
{
    public class Program
    {
        public interface IFoo
        {
            int Brill();
            string Baz(string param);
            string Bar(int a, int b);
        }

        static void Main(string[] args)
        {
            TestRunner.PrintResults(Console.Out, TestRunner.RunAllTests());
        }

        [Test]
        public static void PassingTest(ITestContext testContext)
        {
            testContext.Assert(0 == 0, "Zero should equal zero!");
        }

        [Test]
        public static void FailingTest(ITestContext testContext)
        {
            testContext.Assert(1 == 0, "One should equal zero, dummy!");
        }

        [Test]
        public static void ExceptionTest(ITestContext textContext)
        {
            textContext.RunExpectingException<InvalidOperationException>(() =>
            {
                throw new InvalidOperationException();
            }, "This test should throw an Invalid Operation Exception!");
        }

        [Test]
        public static void FailingExceptionTest(ITestContext testContext)
        {
            testContext.RunExpectingException<InvalidOperationException>(() =>
            {

            }, "This test isn't throwing an Invalid Operation Exception like it's expected to!");
        }

        [Test]
        public static void MockTest(ITestContext testContext)
        {
            Substitute<IFoo> fooSubstitute = new Substitute<IFoo>();

            IFoo foo = fooSubstitute.MockObject;

            // Test setting the handler of a 0-param function
            fooSubstitute.SetMethodHandler(nameof(IFoo.Brill), () => 10);
            testContext.Assert(foo.Brill() == 10, "The mock handler should return 10!");

            // Test setting the default handler of a 2-param function
            fooSubstitute.SetMethodDefaultHandler(nameof(IFoo.Bar), (int a, int b) => $"Default mock Bar of {a} and {b}");
            testContext.Assert(foo.Bar(4, 7) == "Default mock Bar of 4 and 7", "The default mock handler should return this string!");

            // Test setting a specific handler of a 2-param function
            fooSubstitute.SetMethodHandler(nameof(IFoo.Bar), 5, 8, () => "Specialized mock Bar of 5 and 8");
            testContext.Assert(foo.Bar(5, 8) == "Specialized mock Bar of 5 and 8", "The specialized mock handler should return this string!");
            // Test that the default handler still handles other cases
            testContext.Assert(foo.Bar(4, 7) == "Default mock Bar of 4 and 7", "The default mock handler should return this string!");
        }
    }
}
