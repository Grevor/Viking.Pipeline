using NUnit.Framework;
using System;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class SmallPipelineTests
    {
        [Test]
        public void SmallPipelineTest1()
        {
            var value1 = Assignable(1);
            var value2 = Assignable(2);

            var multiply = PipelineOperations.Create("", (a, b) => a * b, value1, value2).AttachTestStage();
            var cache = multiply.WithCache().AttachTestStage();
            var equality = cache.WithEqualityCheck().AttachTestStage();

            multiply.AssertNotInvalidatedNorRetrieved();
            cache.AssertNotInvalidatedNorRetrieved();
            equality.AssertNotInvalidatedNorRetrieved();


            var value = equality.GetValue();
            multiply.AssertInvalidationsAndRetrievals(0, 1);
            cache.AssertInvalidationsAndRetrievals(0, 1);
            equality.AssertInvalidationsAndRetrievals(0, 1);

            PipelineAssert.Value(multiply, 2);
            PipelineAssert.Value(cache, 2);
            PipelineAssert.Value(equality, 2);

            multiply.AssertInvalidationsAndRetrievals(0, 2);
            cache.AssertInvalidationsAndRetrievals(0, 3);
            equality.AssertInvalidationsAndRetrievals(0, 2);

            value1.SetValue(3);

            multiply.AssertInvalidationsAndRetrievals(1, 3);
            cache.AssertInvalidationsAndRetrievals(1, 4);
            equality.AssertInvalidationsAndRetrievals(1, 2);
            PipelineAssert.Value(equality, 6);


            value1.SetValueWithoutInvalidating(6);
            value2.SetValueWithoutInvalidating(1);
            PipelineCore.Invalidate(value1, value2);

            multiply.AssertInvalidationsAndRetrievals(2, 4);
            cache.AssertInvalidationsAndRetrievals(2, 6);
            equality.AssertInvalidationsAndRetrievals(1, 3);

            PipelineAssert.Value(equality, 6);

            GC.KeepAlive(equality);
        }

        [Test]
        public void SmallPipelineTest2()
        {
            var value1 = Assignable(2);
            var value2 = Assignable(3);
            var value3 = Assignable(-10);

            var constant1 = 100.AsPipelineConstant();
            var constant2 = (-1).AsPipelineConstant();

            var op1 = PipelineOperations.Create("", (a, b) => a * a + b, value1, constant1);
            var op2 = PipelineOperations.Create("", (a, b, c) => (a - b) * c + 1, value2, value1, constant2);

            var result = PipelineOperations.Create("", (a, b, c) => a + b + c, op1, op2, value3);

            PipelineAssert.Value(result, 94);
        }







        private static AssignablePipelineStage<T> Assignable<T>(T initial) => Assignable("", initial);
        private static AssignablePipelineStage<T> Assignable<T>(string name, T initial) => new AssignablePipelineStage<T>(name, initial);
    }
}
