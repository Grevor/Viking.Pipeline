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

            var numStages = source.GetAllDependentStages().Count();
            Assert.Less(numStages, 10000);
            Assert.Pass($"Number of stages created is 10001, but only {numStages} is still living.");

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
