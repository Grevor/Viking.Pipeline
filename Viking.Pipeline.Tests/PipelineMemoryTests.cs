using NUnit.Framework;
using System;
using System.Linq;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class PipelineMemoryTests
    {
        [Test]
        public void GarbageCollectedStagesAreRecalimedProperly()
        {
            var source = 1.AsPipelineConstant();

            for (int i = 0; i < 10000; i++)
                new PassThroughPipelineStage<int>("stage " + i, source);

            for (int i = 0; i < 100; ++i)
                CreateMemoryPressure(1024 * 1024);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            Assert.Less(source.GetAllDependentStages().Count(), 10000);

            GC.KeepAlive(source);
        }



        private static byte[] CreateMemoryPressure(int bytes)
        {
            const int PageStride = 4096;
            var memory = new byte[bytes];
            for (int i = 0; i < memory.Length; i += PageStride)
                memory[i] = 1;
            return memory;
        }
    }
}
