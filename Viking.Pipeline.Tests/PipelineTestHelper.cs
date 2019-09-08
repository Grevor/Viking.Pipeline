using Moq;
using Viking.Pipeline.Patterns;

namespace Viking.Pipeline.Tests
{
    public static class PipelineTestHelper
    {
        public static AssignablePipelineStage<T> Assignable<T>(T initial) => new AssignablePipelineStage<T>("", initial);

        public static Mock<IDeferredTransactionControl> TransactionControlMock() => new Mock<IDeferredTransactionControl>(MockBehavior.Loose);
    }
}
