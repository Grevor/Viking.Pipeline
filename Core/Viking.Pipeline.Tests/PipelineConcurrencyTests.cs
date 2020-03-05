using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class PipelineConcurrencyTests
    {

        [Test]
        public void PotentialConcurrentUpdateOfStageIsCorrectlyDiscovered()
        {
            using var goAhead = new ManualResetEvent(false);

            var value1 = new AssignablePipelineStage<int>("", 1000);
            var value2 = new AssignablePipelineStage<int>("", 2000);

            var concurrent = PipelineOperations.Create("Concurrent Stage", (a, b) => Wait(goAhead, 1000), value1, value2).AsEager();

            Task.Run(() => value1.SetValue(1001));
            goAhead.WaitOne();
            var message = Assert.Throws<PipelineException>(() => value2.SetValue(1001));
        }

        [Test]
        public void ConcurrentUpdateOfTwoSeparateGraphsAreNotLabeledAsConcurrent()
        {
            using var goAhead = new ManualResetEvent(false);
            var v1 = new AssignablePipelineStage<int>("", 1);
            var v2 = new AssignablePipelineStage<int>("", 3);

            var g1 = PipelineReactions.Create(_ => Wait(goAhead, 1000), v1);
            var g2 = PipelineReactions.Create(_ => Wait(goAhead, 1000), v2);

            Task.Run(() => v1.SetValue(1001));
            goAhead.WaitOne();
            Assert.DoesNotThrow(() => v2.SetValue(1001));
        }

        public static int Wait(ManualResetEvent ev, int ms)
        {
            ev.Set();
            Task.Delay(ms).Wait();
            return ms;
        }
    }
}
