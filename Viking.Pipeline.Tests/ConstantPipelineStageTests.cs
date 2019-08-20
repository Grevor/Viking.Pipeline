using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class ConstantPipelineStageTests
    {
        private const string TestString = "string value";

        [TestCase("string value")]
        [TestCase("another string value")]
        public void TheConstantValueIsCorrectlyReflected(string value) => PipelineAssert.Value(new ConstantPipelineStage<string>(value), value);

        [TestCase(-44)]
        [TestCase(1337)]
        public void DefaultNameIsTheValueItself(int value) => PipelineAssert.Name(new ConstantPipelineStage<int>(value), value.ToString());

        public void InvalidationFunctionsCorrectly() => PipelineAssert.AssertPipelineIsInvalidatingDependentStages(new ConstantPipelineStage<int>(100));
    }
}
