using NUnit.Framework;
using System.Linq;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class MutuallyExclusivePipelineStageTests
    {

        [Test]
        public void ExceptionOnNullInputToConstructor()
        {
            var valid = 1.AsPipelineConstant();

            PipelineAssert.NullArgumentException(() => new MutuallyExclusivePipelineStage<int>(null, new IPipelineStage[] { }), "input");
            PipelineAssert.NullArgumentException(() => new MutuallyExclusivePipelineStage<int>(valid, null), "dependencies");
            PipelineAssert.NullArgumentException(() => new MutuallyExclusivePipelineStage<int>(valid, new IPipelineStage[] { null }), "single dependency");
            PipelineAssert.NullArgumentException(() => new MutuallyExclusivePipelineStage<int>(null, null), "input and dependencies");
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void InputAndAllExclusiveStagesAreAddedAsDependencies(int numExclusive)
        {
            var input = 10.AsPipelineConstant();
            var exclusive = Enumerable.Range(0, numExclusive).Select(i => i.AsPipelineConstant()).ToArray();
            var sut = new MutuallyExclusivePipelineStage<int>(input, exclusive);

            PipelineAssert.DependentOn(sut, exclusive.Append(input).ToArray());
        }

        [Test]
        public void StagePassesPreviousValueThrough()
        {
            var previous = 10.AsPipelineConstant();
            var exclusive = 100.AsPipelineConstant();
            var sut = new MutuallyExclusivePipelineStage<int>(previous, exclusive);

            PipelineAssert.Value(sut, previous.GetValue());
        }

        [Test]
        public void StageIsInvalidatedWhenNoMutuallyExclusiveStagesAre()
        {
            var previous = 10.AsPipelineConstant();
            var exclusive = 100.AsPipelineConstant();
            var sut = new MutuallyExclusivePipelineStage<int>(previous, exclusive);
            var test = sut.AttachTestStage();

            previous.Invalidate();
            test.AssertInvalidations(1);

            sut.Invalidate();
            test.AssertInvalidations(2);
        }

        [TestCase(1,1)]
        [TestCase(2,1)]
        [TestCase(10,3)]
        [TestCase(10,9)]
        public void StageIsNotInvalidatedWhenAtLeastOneMutuallyExclusiveStageIsInvalidated(int totalExclusiveStages, int invalidations)
        {
            var previous = 10.AsPipelineConstant();
            var exclusive = Enumerable.Range(0, totalExclusiveStages).Select(i => i.AsPipelineConstant()).ToArray();
            var sut = new MutuallyExclusivePipelineStage<int>(previous, exclusive);
            var test = sut.AttachTestStage();

            PipelineCore.Invalidate(new[] { previous }.Concat(exclusive.Take(invalidations)));
            test.AssertInvalidations(0);

            PipelineCore.Invalidate(new[] { sut }.Concat(exclusive.Take(invalidations)));
            test.AssertInvalidations(0);
        }
    }
}
