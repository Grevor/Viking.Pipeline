using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;

namespace Viking.Pipeline.Tests
{
    public static class PipelineAssert
    {
        public static void AssertPipelineIsInvalidatingDependentStages(IPipelineStage stage)
        {
            var t = new Test();
            var reaction = new ReactionPipelineStage(t.Reaction);

            stage.Invalidate();
            Assert.AreEqual(1, t.Count);

            GC.KeepAlive(t);
        }

        public static void Value<T>(IPipelineStage<T> stage, T expected) => Assert.AreEqual(expected, stage.GetValue());
        public static void Name(IPipelineStage stage, string expected) => Assert.AreEqual(expected, stage.Name);
        public static void NameContains(IPipelineStage stage, string contains) => StringAssert.Contains(contains, stage.Name);

        public static TestPipelineStage<T> AttachTestStage<T>(this IPipelineStage<T> stage) => new TestPipelineStage<T>(stage);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string KeepAlive(IPipelineStage stage)
        {
            return stage.Name;
        }

        private class Test
        {
            public int Count { get; private set; }
            public void Reaction() => Count++;
        }
    }
}
