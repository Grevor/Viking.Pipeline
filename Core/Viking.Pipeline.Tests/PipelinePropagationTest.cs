using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class PipelinePropagationTest
    {
        [Test]
        public void LinearPipelineIsInvalidatedInOrder()
        {
            var assignable = Assignable(0);
            var t1 = assignable.AttachTestStage();
            var t2 = PipelineOperations.Create("", a => a, t1).AttachTestStage();
            var t3 = t2.WithCache().AttachTestStage();
            var t4 = PipelineOperations.Create("", a => a, t3).AttachTestStage();

            assignable.SetValue(1);

            t1.AssertInvalidatedBefore(t2);
            t2.AssertInvalidatedBefore(t3);
            t3.AssertInvalidatedBefore(t4);
            PipelineAssert.Value(t4, 1);
        }

        [Test]
        public void StagesInADiamondPipelineAreCalledOnceEachInOrder()
        {
            var assignable = Assignable(0);
            var t1 = assignable.AttachTestStage();
            var d1 = t1.WithNewName("").AttachTestStage();
            var d2 = t1.WithNewName("").AttachTestStage();
            var t2 = PipelineOperations.Create("", (a, b) => a + b, d1, d2).AttachTestStage();

            assignable.SetValue(1);

            t1.AssertInvalidations(1);
            d1.AssertInvalidations(1);
            d2.AssertInvalidations(1);
            t2.AssertInvalidations(1);

            t1.AssertInvalidatedBefore(d1, d2);
            d1.AssertInvalidatedBefore(t2);
            d2.AssertInvalidatedBefore(t2);

            t2.GetValue();

            t1.AssertRetrievals(2);
            d1.AssertRetrievals(1);
            d2.AssertRetrievals(1);
            t2.AssertRetrievals(1);
        }

        private static AssignablePipelineStage<int> Assignable(int initial) => new AssignablePipelineStage<int>("", initial);
    }
}
