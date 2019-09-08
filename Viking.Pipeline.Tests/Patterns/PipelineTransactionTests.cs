using NUnit.Framework;
using System.Linq;
using Viking.Pipeline.Patterns;

namespace Viking.Pipeline.Tests.Patterns
{
    [TestFixture]
    public class PipelineTransactionTests
    {
        [Test]
        public void NoUpdateIsDoneWhenNoStagesHaveBeenMarkedForUpdate()
        {
            var stage = Assignable(10);
            var test = stage.AttachTestStage();

            new PipelineTransaction().Commit();

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

            var sut = new PipelineTransaction();
            foreach (var stage in stages.Take(updates))
                sut.Update(stage, numStages);
            sut.Commit();

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

            new PipelineTransaction()
                .Update(value, 1)
                .Commit();

            test.AssertNotInvalidatedNorRetrieved();
        }

        [Test]
        public void MultipleUpdatesToSameStageCausesOnlyLastToBeUsed()
        {
            var value = Assignable(1);
            var test = value.AttachTestStage();

            new PipelineTransaction()
                .Update(value, 2)
                .Update(value, 3)
                .Update(value, 4)
                .Commit();

            PipelineAssert.Value(value, 4);
            test.AssertInvalidations(1);
        }

        [Test]
        public void TransactionIsAlwaysCommittedSucessfullyOnCompletedCommit()
        {
            Assert.AreEqual(PipelineTransactionResult.Success, new PipelineTransaction().Commit());
            Assert.AreEqual(PipelineTransactionResult.Success, new PipelineTransaction().Update(1.AsPipelineConstant()).Commit());
        }

        [Test]
        public void InvalidationHappensOnlyOnComplete()
        {
            var value = Assignable(1);
            var other = Assignable("hello");

            var test1 = value.AttachTestStage();
            var test2 = other.AttachTestStage();

            var sut = new PipelineTransaction()
                .Update(value, 2)
                .Update(other);

            test1.AssertNotInvalidatedNorRetrieved();
            test2.AssertNotInvalidatedNorRetrieved();

            sut.Commit();

            test1.AssertInvalidations(1);
            test2.AssertInvalidations(1);
        }

        [Test]
        public void CanceledTransactionDoesNotInvokeAnyOfTheUpdates()
        {
            var value = Assignable(1);
            var other = Assignable("hello");

            var test1 = value.AttachTestStage();
            var test2 = other.AttachTestStage();

            var sut = new PipelineTransaction()
                .Update(value, 2)
                .Update(other, "hello 2");

            sut.Cancel();

            test1.AssertNotInvalidatedNorRetrieved();
            test2.AssertNotInvalidatedNorRetrieved();
            PipelineAssert.Value(value, 1);
            PipelineAssert.Value(other, "hello");
        }

        private static AssignablePipelineStage<T> Assignable<T>(T initial) => new AssignablePipelineStage<T>("", initial);
    }
}
