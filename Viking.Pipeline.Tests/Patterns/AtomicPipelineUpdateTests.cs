using NUnit.Framework;
using System.Linq;
using Viking.Pipeline.Patterns;

namespace Viking.Pipeline.Tests.Patterns
{
    [TestFixture]
    public class AtomicPipelineUpdateTests
    {
        [Test]
        public void NoUpdateIsDoneWhenNoStagesHaveBeenMarkedForUpdate()
        {
            var stage = Assignable(10);
            var test = stage.AttachTestStage();

            new AtomicPipelineUpdate().Complete();

            test.AssertInvalidations(0);
        }

        [TestCase(0,0)]
        [TestCase(10, 0)]
        [TestCase(10, 3)]
        [TestCase(30, 30)]
        public void MultipleUpdatesAreAllPropagated(int numStages, int updates)
        {
            var stages = Enumerable.Range(0, numStages).Select(i => Assignable(i)).ToArray();
            var tests = stages.Select(s => s.AttachTestStage()).ToArray();

            var sut = new AtomicPipelineUpdate();
            foreach (var stage in stages.Take(updates))
                sut.Update(stage, numStages);
            sut.Complete();

            foreach (var test in tests.Take(updates))
                test.AssertInvalidations(1);

            foreach (var test in tests.Skip(updates))
                test.AssertNotInvalidatedNorRetrieved();
        }

        [Test]
        public void WhenValueOfAssignablePipelineIsEqualToLastItIsNotAddedForUpdate()
        {
            var value = Assignable(1);
            var test = value.AttachTestStage();

            new AtomicPipelineUpdate()
                .Update(value, 1)
                .Complete();

            test.AssertNotInvalidatedNorRetrieved();
        }

        [Test]
        public void InvalidationHappensOnlyOnComplete()
        {
            var value = Assignable(1);
            var other = Assignable("hello");

            var test1 = value.AttachTestStage();
            var test2 = other.AttachTestStage();

            var sut = new AtomicPipelineUpdate()
                .Update(value, 2)
                .Update(other);

            test1.AssertNotInvalidatedNorRetrieved();
            test2.AssertNotInvalidatedNorRetrieved();

            sut.Complete();

            test1.AssertInvalidations(1);
            test2.AssertInvalidations(1);
        }

        private static AssignablePipelineStage<T> Assignable<T>(T initial) => new AssignablePipelineStage<T>("", initial);
    }
}
