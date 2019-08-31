using NUnit.Framework;
using System.Linq;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class PipelinePropagationInformationStageTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        public void InputsAreAddedAsDependencies(int stages)
        {
            var input = Enumerable.Range(0, stages).Select(i => i.AsPipelineConstant()).ToArray();
            var sut = new PipelinePropagationInformationStage<int>("", inv => 1, -1, input);

            PipelineAssert.Dependencies(sut, input);
        }

        [Test]
        public void InitialInformationIsGivenWhenNoInvalidationHasHappened()
        {
            var sut = new PipelinePropagationInformationStage<string>("", inv => "Hello, World!", "");
            PipelineAssert.Value(sut, "");
        }

        [Test]
        public void AllDependenciesAreInvalidatedAtInformationExtractionTime()
        {
            var a = 1.AsPipelineConstant().AttachTestStage();
            var b = 2.AsPipelineConstant().AttachTestStage();
            var sut = new PipelinePropagationInformationStage<string>("", inv => "Hello, World!", "", a, b);
            var test = sut.AttachTestStage();

            PipelineCore.Invalidate(a, b);
            test.AssertStageInvalidated(a);
            test.AssertStageInvalidated(b);
        }

        [Test]
        public void EachNewInvalidationPicksUpNewInformation()
        {
            var a = new AssignablePipelineStage<int>("", 1);
            var sut = new PipelinePropagationInformationStage<int>("", inv => a.GetValue(), a.GetValue(), a);
            var test = sut.AttachTestStage();

            PipelineAssert.Value(sut, 1);

            a.SetValue(2);
            test.AssertInvalidations(1);
            PipelineAssert.Value(sut, 2);
            PipelineAssert.Value(sut, 2);

            a.SetValue(5);
            test.AssertInvalidations(2);
            PipelineAssert.Value(sut, 5);
            PipelineAssert.Value(sut, 5);
        }
    }
}
