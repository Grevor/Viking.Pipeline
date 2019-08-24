using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class SuspendingPipelineStageTests
    {

        public void PipelineStartsInExpectedState()
        {
            var sut = new SuspendingPipelineStage<double>(0.2.AsPipelineConstant(), Suspender(PipelineSuspensionState.Resume));
            Assert.AreEqual(PipelineSuspensionState.Resume, sut.SuspensionState);
        }

        [TestCase(PipelineSuspensionState.Resume, 1)]
        [TestCase(PipelineSuspensionState.ResumeWithoutPendingInvalidates, 1)]
        [TestCase(PipelineSuspensionState.Suspend, 0)]
        public void InvalidateIsOnlyPropagatedInResumedSuspensionStates(PipelineSuspensionState state, int expectedInvalidations)
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
            var sut = new SuspendingPipelineStage<int>(value, Suspender(PipelineSuspensionState.Suspend));

            value.SetValue(11);
            Assert.IsTrue(sut.HasPendingInvalidate);
        }

        [TestCase(PipelineSuspensionState.Resume, 1)]
        [TestCase(PipelineSuspensionState.ResumeWithoutPendingInvalidates, 0)]
        [TestCase(PipelineSuspensionState.Suspend, 0)]
        public void PendingInvalidateIsEnactedWhenChangingState(PipelineSuspensionState resumeState, int expectedInvalidations)
        {
            var value = Assignable(10);
            var state = Suspender(PipelineSuspensionState.Suspend);
            var sut = new SuspendingPipelineStage<int>(value, state);
            var test = sut.AttachTestStage();

            value.SetValue(2);
            test.AssertInvalidations(0);
            Assert.IsTrue(sut.HasPendingInvalidate);

            state.SetValue(resumeState);
            test.AssertInvalidations(expectedInvalidations);
        }

        [TestCase(PipelineSuspensionState.Resume)]
        [TestCase(PipelineSuspensionState.ResumeWithoutPendingInvalidates)]
        public void ResumeWithoutInvalidationFromInputDoesNotInvalidateStage(PipelineSuspensionState resumeState)
        {
            var state = Suspender(PipelineSuspensionState.Suspend);
            var sut = new SuspendingPipelineStage<int>(1.AsPipelineConstant(), state);
            var test = sut.AttachTestStage();

            state.SetValue(resumeState);
            test.AssertInvalidations(0);
        }


        private static AssignablePipelineStage<PipelineSuspensionState> Suspender(PipelineSuspensionState state) => new AssignablePipelineStage<PipelineSuspensionState>("", state);
        private static AssignablePipelineStage<T> Assignable<T>(T initial) => new AssignablePipelineStage<T>("", initial);
    }
}
