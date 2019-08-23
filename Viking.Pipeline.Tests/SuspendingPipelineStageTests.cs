using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class SuspendingPipelineStageTests
    {

        public void PipelineStartsInExpectedState()
        {
            var sut = new SuspendingPipelineStage<double>(0.2.AsPipelineConstant(), Suspender(PipelineSuspension.Resume));
            Assert.AreEqual(PipelineSuspension.Resume, sut.SuspensionState);
        }

        [TestCase(PipelineSuspension.Resume, 1)]
        [TestCase(PipelineSuspension.ResumeWithoutPendingInvalidates, 1)]
        [TestCase(PipelineSuspension.Suspend, 0)]
        public void InvalidateIsOnlyPropagatedInResumedSuspensionStates(PipelineSuspension state, int expectedInvalidations)
        {
            var value = Assignable(10);
            var sut = new SuspendingPipelineStage<int>(value, Suspender(state));
            var test = sut.AttachTestStage();

            value.SetValue(11);
            test.AssertInvalidations(expectedInvalidations);
        }

        [Test]
        public void ValueInvalidateWhileInSuspendedStateIsRememberedAsPending()
        {
            var value = Assignable(10);
            var sut = new SuspendingPipelineStage<int>(value, Suspender(PipelineSuspension.Suspend));

            value.SetValue(11);
            Assert.IsTrue(sut.HasPendingInvalidate);
        }

        [TestCase(PipelineSuspension.Resume, 1)]
        [TestCase(PipelineSuspension.ResumeWithoutPendingInvalidates, 0)]
        [TestCase(PipelineSuspension.Suspend, 0)]
        public void PendingInvalidateIsEnactedWhenChangingState(PipelineSuspension resumeState, int expectedInvalidations)
        {
            var value = Assignable(10);
            var state = Suspender(PipelineSuspension.Suspend);
            var sut = new SuspendingPipelineStage<int>(value, state);
            var test = sut.AttachTestStage();

            value.SetValue(2);
            test.AssertInvalidations(0);
            Assert.IsTrue(sut.HasPendingInvalidate);

            state.SetValue(resumeState);
            test.AssertInvalidations(expectedInvalidations);
        }

        [TestCase(PipelineSuspension.Resume)]
        [TestCase(PipelineSuspension.ResumeWithoutPendingInvalidates)]
        public void ResumeWithoutInvalidationFromInputDoesNotInvalidateStage(PipelineSuspension resumeState)
        {
            var state = Suspender(PipelineSuspension.Suspend);
            var sut = new SuspendingPipelineStage<int>(1.AsPipelineConstant(), state);
            var test = sut.AttachTestStage();

            state.SetValue(resumeState);
            test.AssertInvalidations(0);
        }


        private static AssignablePipelineStage<PipelineSuspension> Suspender(PipelineSuspension state) => new AssignablePipelineStage<PipelineSuspension>("", state);
        private static AssignablePipelineStage<T> Assignable<T>(T initial) => new AssignablePipelineStage<T>("", initial);
    }
}
