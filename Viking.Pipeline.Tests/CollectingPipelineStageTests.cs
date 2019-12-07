using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class CollectingPipelineStageTests
    {
        public void NullInputToConstructorThrowsException()
        {
            PipelineAssert.NullArgumentException(() => new CollectingPipelineStage<int>(null, Enumerable.Empty<IPipelineStage<int>>()), "Name");
            PipelineAssert.NullArgumentException(() => new CollectingPipelineStage<int>("stage name", null), "initial inputs");
            PipelineAssert.NullArgumentException(() => new CollectingPipelineStage<int>("stage name", new IPipelineStage<int>[] { null }), "initial input pipeline null");
        }

        [Test]
        public void AddingInputInvalidatesStage()
        {
            var a = 1.AsPipelineConstant();
            var sut = new CollectingPipelineStage<int>("", new[] { a });
            var test = sut.AttachTestStage();

            sut.AddInputs(new[] { 2.AsPipelineConstant() });

            test.AssertInvalidations(1);
        }

        [Test]
        public void AddingSameInputAgainDoesNotInvalidateStage()
        {
            var a = 1.AsPipelineConstant();
            var sut = new CollectingPipelineStage<int>("", new[] { a });
            var test = sut.AttachTestStage();

            sut.AddInputs(new[] { a });

            test.AssertInvalidations(0);
        }

        [Test]
        public void RemovingInputInvalidatesStage()
        {
            var a = 1.AsPipelineConstant();
            var sut = new CollectingPipelineStage<int>("", new[] { a });
            var test = sut.AttachTestStage();

            sut.RemoveInputs(new[] { a });

            test.AssertInvalidations(1);
        }

        [Test]
        public void RemovingNonInputDoesNotInvalidateStage()
        {
            var a = 1.AsPipelineConstant();
            var sut = new CollectingPipelineStage<int>("", new[] { a });
            var test = sut.AttachTestStage();

            sut.RemoveInputs(new[] { 2.AsPipelineConstant() });

            test.AssertInvalidations(0);
        }

        [Test]
        public void InvalidationOfInputsInvalidatesStage()
        {
            var a = PipelineTestHelper.Assignable(1);
            var sut = new CollectingPipelineStage<int>("", new[] { a });
            var test = sut.AttachTestStage();

            a.Invalidate();
            test.AssertInvalidations(1);
        }

        [Test]
        public void InvalidationOfAddedInputsInvalidatesStage()
        {
            var a = PipelineTestHelper.Assignable(1);
            var b = PipelineTestHelper.Assignable(2);
            var sut = new CollectingPipelineStage<int>("", new[] { a });
            sut.AddInputs(new[] { b });
            var test = sut.AttachTestStage();

            b.Invalidate();
            test.AssertInvalidations(1);
        }

        [Test]
        public void InvalidationOfRemovedInputDoesNotInvalidateTheStage()
        {
            var a = PipelineTestHelper.Assignable(1);
            var sut = new CollectingPipelineStage<int>("", new[] { a });
            sut.RemoveDependencies(new[] { a });
            var test = sut.AttachTestStage();

            a.Invalidate();
            test.AssertInvalidations(0);
        }
    }
}
