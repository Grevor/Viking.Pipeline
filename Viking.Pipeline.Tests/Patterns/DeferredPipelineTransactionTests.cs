using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using Viking.Pipeline.Patterns;
using System.Linq;
using System;

namespace Viking.Pipeline.Tests.Patterns
{
    [TestFixture]
    public class DeferredPipelineTransactionTests
    {
        [Test]
        public void ConstructorRegistersWithTheControl()
        {
            var control = PipelineTestHelper.TransactionControlMock();
            var sut = new DeferredPipelineTransaction(control.Object);

            control.Verify(mock => mock.Register(It.Is<DeferredPipelineTransaction>(a => a == sut)), Times.Once);
        }

        [Test]
        public void TransactionDoesNothingBeforeCommit()
        {
            var value = PipelineTestHelper.Assignable(1);
            var test = value.AttachTestStage();
            var control = PipelineTestHelper.TransactionControlMock();
            control.Setup(mock => mock.Commit(It.IsAny<DeferredPipelineTransaction>(), It.IsAny<IEnumerable<DeferredTransactionPart>>()))
                .Callback(new Action<DeferredPipelineTransaction, IEnumerable<DeferredTransactionPart>>((a, b) => { var c = new AggregatingTransactionControl(); c.Register(a); c.Commit(a, b); }));

            var sut = new DeferredPipelineTransaction(control.Object)
                .Update(value, 2);

            control.Verify(mock => mock.Commit(It.IsAny<DeferredPipelineTransaction>(), It.IsAny<IEnumerable<DeferredTransactionPart>>()), Times.Never);
            PipelineAssert.Value(value, 1);
            test.AssertNotInvalidatedNorRetrieved();

            sut.Commit();
            control.Verify(mock => mock.Commit(It.IsAny<DeferredPipelineTransaction>(), It.IsAny<IEnumerable<DeferredTransactionPart>>()), Times.Once);
            PipelineAssert.Value(value, 2);
            test.AssertInvalidations(1);
        }

        [Test]
        public void TransactionCannotBeUsedAfterBeingCommitted()
        {
            var control = PipelineTestHelper.TransactionControlMock();
            var sut = new DeferredPipelineTransaction(control.Object);

            sut.Commit();

            Assert.Throws<InvalidOperationException>(() => sut.Update(1.AsPipelineConstant(), () => true));
            Assert.Throws<InvalidOperationException>(() => sut.Commit());
            Assert.Throws<InvalidOperationException>(() => sut.Cancel());
        }

        [Test]
        public void CanceledTransactionDoesNotInvokeAnyOfTheUpdates()
        {
            var value = PipelineTestHelper.Assignable(1);
            var other = PipelineTestHelper.Assignable("hello");

            var test1 = value.AttachTestStage();
            var test2 = other.AttachTestStage();

            var control = PipelineTestHelper.TransactionControlMock();
            var sut = new DeferredPipelineTransaction(control.Object)
                .Update(value, 2)
                .Update(other, "hello 2");

            sut.Cancel();

            test1.AssertNotInvalidatedNorRetrieved();
            test2.AssertNotInvalidatedNorRetrieved();
            PipelineAssert.Value(value, 1);
            PipelineAssert.Value(other, "hello");
        }

        [Test]
        public void ControlIsCalledOnCommitWithTheExpectedStage()
        {
            var value = PipelineTestHelper.Assignable(1);
            var control = PipelineTestHelper.TransactionControlMock();
            var sut = new DeferredPipelineTransaction(control.Object)
                .Update(value, 2);

            control.Setup(mock => mock.Commit(
                It.IsAny<DeferredPipelineTransaction>(),
                It.IsAny<IEnumerable<DeferredTransactionPart>>()))
                .Callback<DeferredPipelineTransaction,IEnumerable<DeferredTransactionPart>>((a,b)=> Assert.IsTrue(a == sut && b.Count() == 1 && b.First().Stage == value));

            sut.Commit();
        }
    }
}
