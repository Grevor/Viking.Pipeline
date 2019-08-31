using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class DetachablePipelineStageTests
    {
        [Test]
        public void ExceptionOnNullInputToConstructor() => PipelineAssert.NullArgumentException(() => new DetachablePipelineStage<int>(null), "input");

        [Test]
        public void InputIsAddedAsDependency()
        {
            var input = 10.AsPipelineConstant();
            var sut = new DetachablePipelineStage<int>(input);

            PipelineAssert.DependentOn(sut, input);
        }

        [TestCase(0)]
        [TestCase(600)]
        public void DetachableStagePassesPreviousStagesValueThrough(int i)
        {
            var sut = new DetachablePipelineStage<int>(i.AsPipelineConstant());
            PipelineAssert.Value(sut, i);
        }

        [Test]
        public void DetachableStagePropagatesInvalidatesWhileNotDetached()
        {
            var sut = new DetachablePipelineStage<int>(1.AsPipelineConstant());
            var test = sut.AttachTestStage();

            sut.Invalidate();
            test.AssertInvalidations(1);
        }

        [Test]
        public void DetachableStageNoLongerPropagatesInvalidateAfterBeingDetached()
        {
            var sut = new DetachablePipelineStage<int>(1.AsPipelineConstant());
            var test = sut.AttachTestStage();

            sut.Invalidate();
            test.AssertInvalidations(1);
            sut.DetachFromPipeline();
            test.AssertInvalidations(1);

            sut.Invalidate();
            sut.Invalidate();
            sut.Invalidate();

            test.AssertInvalidations(1);
        }
    }
}
