using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class LazyDeltaPipelineStageTests
    {
        [Test]
        public void ExceptionOnNullInputToConstructor()
        {
            var valid = 1.AsPipelineConstant();
            PipelineAssert.NullArgumentException(() => new LazyDeltaPipelineStage<int, int>(null, Extractor, valid), "name");
            PipelineAssert.NullArgumentException(() => new LazyDeltaPipelineStage<int, int>("", null, valid), "extractor");
            PipelineAssert.NullArgumentException(() => new LazyDeltaPipelineStage<int, int>("", Extractor, null), "input");
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-20)]
        public void InitialBaselineValueIsUsedInInitialDeltaEvaluation(int initial)
        {
            var assignable = PipelineTestHelper.Assignable(initial);
            var sut = new LazyDeltaPipelineStage<int, int>("", Extractor, assignable);
            PipelineAssert.Value(sut, 0);
        }

        [TestCase(10, 11, 1)]
        [TestCase(-10, 34, 44)]
        [TestCase(1, -2, -3)]
        public void ExtractorIsUsedToCalculateNewDelta(int old, int @new, int expected)
        {
            var input = PipelineTestHelper.Assignable(old);
            var sut = new LazyDeltaPipelineStage<int, int>("", Extractor, input);
            var test = sut.AttachTestStage();

            input.SetValue(@new);
            test.AssertInvalidations(1);
            PipelineAssert.Value(sut, expected);
        }

        [Test]
        public void InvalidationIsPropagatedWithoutAnyGetValueBeingCalled()
        {
            var assignable = PipelineTestHelper.Assignable(0);
            var test = assignable.AttachTestStage();
            var sut = new LazyDeltaPipelineStage<int, int>("", Extractor, test);
            var sutTest = sut.AttachTestStage();

            test.AssertInvalidationsAndRetrievals(0, 1);

            assignable.Invalidate();
            test.AssertInvalidationsAndRetrievals(1, 1);
            sutTest.AssertInvalidationsAndRetrievals(1, 0);
        }

        [Test]
        public void InputIsAddedAsDependency()
        {
            var input = 1.AsPipelineConstant();
            var sut = new DeltaPipelineStage<int, int>("", Extractor, input, -2);

            PipelineAssert.DependentOn(sut, input);
        }



        private static int Extractor(int old, int @new) => @new - old;
    }
}
