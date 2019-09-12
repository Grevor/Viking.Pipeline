using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class ThreadSafePipelineStageTests
    {
        [Test]
        public void ExceptionOnNullInputToConstructor()
        {
            PipelineAssert.NullArgumentException(() => new ThreadSafePipelineStage<int>(null), "input");
        }

        [Test]
        public void InputIsRegisteredAsDependency()
        {
            var input = 1.AsPipelineConstant();
            var sut = new ThreadSafePipelineStage<int>(input);

            PipelineAssert.DependentOn(sut, input);
        }

        [Test]
        public void InvalidationsArePropagated()
        {
            var sut = new ThreadSafePipelineStage<int>(1.AsPipelineConstant());
            var test = sut.AttachTestStage();

            PipelineCore.Invalidate(sut);

            test.AssertInvalidations(1);
        }

        [Test]
        [Repeat(3)]
        public void ConcurrentRetrievalsRetrievesOnlyOnceFromUpstreamStage()
        {
            using (EventWaitHandle completeRetrievalFlag = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                using (var semaphore = new SemaphoreSlim(0))
                {
                    var input = new DataRetrievalPipelineStage<object>("", () => { completeRetrievalFlag.WaitOne(); return new object(); });
                    var inputTest = input.AttachTestStage();

                    var sut = new ThreadSafePipelineStage<object>(inputTest);

                    object GetObject()
                    {
                        semaphore.Release();
                        return sut.GetValue();
                    }

                    var task1 = Task.Run(GetObject);
                    var task2 = Task.Run(GetObject);

                    var oneSecond = TimeSpan.FromSeconds(1);
                    Assert.IsTrue(semaphore.Wait(oneSecond));
                    Assert.IsTrue(semaphore.Wait(oneSecond));

                    completeRetrievalFlag.Set();

                    Assert.IsTrue(Task.WaitAll(new[] { task1, task2 }, oneSecond));

                    var result1 = task1.Result;
                    var result2 = task2.Result;

                    Assert.AreSame(result1, result2);
                }
            }
        }

        [Test]
        [Repeat(3)]
        public void OnlyOneThreadIsInsideThePreviousPipelineStagesGetValueFunctionAtATime()
        {
            var container = new Container();
            int EnsureOnlyOne()
            {
                if (container.Thread != null)
                    throw new Exception();

                container.Thread = Thread.CurrentThread;

                if(container.Thread != Thread.CurrentThread)
                    throw new Exception();

                container.Thread = null;
                return 1;
            }

            var input = new DataRetrievalPipelineStage<int>("", EnsureOnlyOne);
            var sut = new ThreadSafePipelineStage<int>(input);
            var tasks = Enumerable.Repeat(0, 1000).Select(_ => Task.Run(sut.GetValue)).ToArray();

            Task.WaitAll(tasks);
            Assert.Pass("No exceptions were thrown, indicating that no concurrency issues were found.");
        }

        private class Container
        {
            public volatile Thread Thread;
        }
    }
}
