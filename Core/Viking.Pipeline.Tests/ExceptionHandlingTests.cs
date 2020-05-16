using NUnit.Framework;
using System;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class ExceptionHandlingTests
    {
        [Test]
        public void ExceptionInPropagationAreHandledAndATraceIsCreated()
        {
            var sut = new DataRetrievalPipelineStage<int>("Exception", () => throw new Exception("EXCEPTION!"));
            var ss = AssignablePipelineStage.Create("Assignable", 10);
            var dd = PipelineOperations.Create("operation", (a, e) => a, ss, sut);

            var sdf = dd.CreateReaction(a => { }, false);

            var exception = Assert.Catch<PipelineException>(() => ss.SetValue(11));
            TestContext.WriteLine(exception.Message);
        }
    }
}
