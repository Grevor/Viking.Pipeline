using NUnit.Framework;
using System;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class EagerPipelineStageTests
    {
        [Test]
        public void InputIsAddedAsDependency()
        {
            var input = 10.AsPipelineConstant();
            var sut = new EagerPipelineStage<int>(input);

            PipelineAssert.Dependencies(sut, input);
        }

        [Test]
        public void EagerStageCausesRetrievalOnInvalidate()
        {
            var input = 1.AsPipelineConstant();
            var test = input.AttachTestStage();
            var sut = new EagerPipelineStage<int>(test);

            input.Invalidate();
            test.AssertInvalidationsAndRetrievals(1, 1);

            input.Invalidate();
            input.Invalidate();
            input.Invalidate();
            test.AssertInvalidationsAndRetrievals(4, 4);

            GC.KeepAlive(sut); // To keep the GC from collecting the eager stage, as it is not used.
        }

        [TestCase(10)]
        [TestCase(-30)]
        public void EagerStagePassesThroughValue(int i)
        {
            var sut = new EagerPipelineStage<int>(i.AsPipelineConstant());

            PipelineAssert.Value(sut, i);
        }
    }
}
