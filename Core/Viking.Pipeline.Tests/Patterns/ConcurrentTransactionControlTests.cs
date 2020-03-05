using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Viking.Pipeline.Patterns;

namespace Viking.Pipeline.Tests.Patterns
{
    [TestFixture]
    public class ConcurrentTransactionControlTests
    {
        [Test]
        public void ControlRefusesToCancelTransactionThatIsNotRegistered()
        {
            var control = new AggregatingTransactionControl();
            var transaction = new DeferredPipelineTransaction(new AggregatingTransactionControl());

            Assert.Throws<ArgumentException>(() => control.Cancel(transaction));
        }

        [Test]
        public void ControlRefusesToCommitTransactionThatIsNotRegistered()
        {
            var control = new AggregatingTransactionControl();
            var transaction = new DeferredPipelineTransaction(new AggregatingTransactionControl());
            Assert.Throws<ArgumentException>(() => control.Commit(transaction, Enumerable.Empty<DeferredTransactionPart>()));
        }

        [TestCase(new[] { 0, 1, 2, 3 }, new[] { 1, 2 }, new[] { 0, 3 })]
        [TestCase(new[] { 0, 1, 2, 3, 4 }, new[] { 1, 3 }, new[] { 0, 2 })]
        [TestCase(new[] { 0, 1, 2, 3, 4 }, new[] { 2, 3 }, new[] { 0, 1 })]
        [TestCase(new[] { 0, 1, 2, 3, 4 }, new[] { 3, 4 }, new[] { 1, 2 })]
        public void ConcurrentTransactionsAreAggregatedToOneTransactionOnceAllTransactionsAreDone(int[] totalOrder, int[] transaction1, int[] transaction2)
        {
            var stages = Enumerable.Repeat(1, totalOrder.Length).Select(_ => PipelineTestHelper.Assignable(1)).ToList();
            var tests = stages.Select(stage => stage.AttachTestStage()).ToList();

            var control = new AggregatingTransactionControl();
            var t1 = control.CreateTransaction();
            var t2 = control.CreateTransaction();

            foreach (var i in totalOrder)
            {
                if (transaction1.Contains(i))
                    t1.Update(stages[i], 2);
                if (transaction2.Contains(i))
                    t2.Update(stages[i], 2);
            }

            AssertExpectedTransactionOutcome(t1, t2, stages, tests, transaction1.Concat(transaction2), PipelineTransactionResult.PendingSuccess, PipelineTransactionResult.Success);
        }

        [TestCase(new[] { 0, 1, 2, 3 }, new[] { 1, 2 }, new[] { 0, 1, 3 })]
        [TestCase(new[] { 0, 1, 2, 3, 4 }, new[] { 1, 4, 3 }, new[] { 1, 2 })]
        public void ConcurrentTransactionsWhichCollideCausesTheLastOneToRollback(int[] totalOrder, int[] transaction1, int[] transaction2)
        {
            var stages = Enumerable.Repeat(1, totalOrder.Length).Select(_ => PipelineTestHelper.Assignable(1)).ToList();
            var tests = stages.Select(stage => stage.AttachTestStage()).ToList();

            var control = new AggregatingTransactionControl();
            var t1 = control.CreateTransaction();
            var t2 = control.CreateTransaction();

            foreach (var i in totalOrder)
            {
                if (transaction1.Contains(i))
                    t1.Update(stages[i], 2);
                if (transaction2.Contains(i))
                    t2.Update(stages[i], 2);
            }

            AssertExpectedTransactionOutcome(t1, t2, stages, tests, transaction1, PipelineTransactionResult.PendingSuccess, PipelineTransactionResult.Failed);
        }

        private static void AssertExpectedTransactionOutcome(
            IPipelineTransaction t1,
            IPipelineTransaction t2,
            List<AssignablePipelineStage<int>> stages,
            List<TestPipelineStage<int>> tests,
            IEnumerable<int> transaction,
            PipelineTransactionResult expectedFirstResult,
            PipelineTransactionResult expectedSecondResult)
        {
            var stagesInTransaction = transaction.Select(i => stages[i]).ToList();
            var testsInTransaction = transaction.Select(i => tests[i]).ToList();

            var nonTransactionIndices = Enumerable.Range(0, stages.Count).Except(transaction).ToList();
            var stagesNotInTransaction = nonTransactionIndices.Select(i => stages[i]).ToList();
            var testsNotInTransaction = nonTransactionIndices.Select(i => tests[i]).ToList();

            Assert.AreEqual(expectedFirstResult, t1.Commit());
            stages.ForEach(a => PipelineAssert.Value(a, 1));
            tests.ForEach(t => t.AssertNotInvalidatedNorRetrieved());

            Assert.AreEqual(expectedSecondResult, t2.Commit());
            stagesInTransaction.ForEach(a => PipelineAssert.Value(a, 2));
            testsInTransaction.ForEach(t => t.AssertInvalidations(1));

            stagesNotInTransaction.ForEach(a => PipelineAssert.Value(a, 1));
            testsNotInTransaction.ForEach(t => t.AssertNotInvalidatedNorRetrieved());
        }
    }
}
