using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class PassivePipelineStageTests
    {
        [Test]
        public void ExceptionOnNullInputToConstructor() => PipelineAssert.NullArgumentException(() => new PassivePipelineStage<int>(null), "input");

        [Test]
        public void InputIsAddedAsDependency()
        {
            var input = 1.AsPipelineConstant();
            var sut = new PassivePipelineStage<int>(input);

            PipelineAssert.DependentOn(sut, input);
        }

        [Test]
        public void PassiveStageIsNeverInvalidated()
        {
            var assignable = new AssignablePipelineStage<int>("", 10);
            var passive = new PassivePipelineStage<int>(assignable);
            var test = passive.AttachTestStage();

            assignable.SetValue(11);
            test.AssertInvalidations(0);
        }
    }
}
