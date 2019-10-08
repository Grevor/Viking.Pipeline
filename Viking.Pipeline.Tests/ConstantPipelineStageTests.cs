using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class ConstantPipelineStageTests
    {
        [Test]
        public void ExceptionOnNullNameToConstructor() => PipelineAssert.NullArgumentException(() => new ConstantPipelineStage<int>(null, 1), "name");

        [TestCase("string value")]
        [TestCase("another string value")]
        public void TheConstantValueIsCorrectlyReflected(string value) => PipelineAssert.Value(new ConstantPipelineStage<string>(value), value);

        [TestCase(-44)]
        [TestCase(1337)]
        public void DefaultNameIsTheValueItself(int value) => PipelineAssert.Name(new ConstantPipelineStage<int>(value), value.ToString());

        [Test]
        public void InvalidationFunctionsCorrectly() => PipelineAssert.AssertPipelineIsInvalidatingDependentStages(new ConstantPipelineStage<int>(100));
    }
}
