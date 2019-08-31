using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class DeltaPipelineStageTests
    {
        [Test]
        public void ExceptionOnNullInputToConstructor()
        {
            var valid = 1.AsPipelineConstant();
            PipelineAssert.NullArgumentException(() => new DeltaPipelineStage<int, int>(null, Extractor, valid, 1), "name");
            PipelineAssert.NullArgumentException(() => new DeltaPipelineStage<int, int>("", null, valid, 1), "extractor");
            PipelineAssert.NullArgumentException(() => new DeltaPipelineStage<int, int>("", Extractor, null, 1), "input");
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-20)]
        public void InitialDeltaValueIsGivenBeforeAnyInvalidations(int value)
        {
            var sut = new DeltaPipelineStage<int, int>("", Extractor, 1.AsPipelineConstant(), value);
            PipelineAssert.Value(sut, value);
        }

        [TestCase(10, 11, 1)]
        [TestCase(-10, 34, 44)]
        [TestCase(1, -2, -3)]
        public void ExtractorIsUsedToCalculateNewDelta(int old, int @new, int expected)
        {
            var input = new AssignablePipelineStage<int>("", old);
            var sut = new DeltaPipelineStage<int, int>("", Extractor, input, expected - 1);
            var test = sut.AttachTestStage();

            PipelineAssert.Value(sut, expected - 1);

            input.SetValue(@new);
            test.AssertInvalidations(1);
            PipelineAssert.Value(sut, expected);
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
