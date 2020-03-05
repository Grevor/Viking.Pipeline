using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class ConcurrentExecutionPipelineStageTests
    {
        [Test]
        public void InputIsRegisteredAsDependency()
        {
            var input = 1.AsPipelineConstant();
            var sut = new ConcurrentExecutionPipelineStage<int>(input);

            PipelineAssert.DependentOn(sut, input);
        }

        [Test]
        public void ExceptionIsThrownOnNullInputToConstructor()
        {
            PipelineAssert.NullArgumentException(() => new ConcurrentExecutionPipelineStage<int>(null), "input");
        }

        [TestCase(0)]
        [TestCase(-3)]
        [TestCase(10)]
        public void InputValueIsPassedThrough(int value)
        {
            var input = value.AsPipelineConstant();
            var sut = new ConcurrentExecutionPipelineStage<int>(input);

            Assert.AreEqual(value, sut.GetValue().Result);
        }
    }
}
