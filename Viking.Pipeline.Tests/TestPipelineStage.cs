using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Viking.Pipeline.Tests
{
    public class TestPipelineStage<T> : IPipelineStage<T>
    {
        private static long InvalidateDatumSource = 0;

        public TestPipelineStage(IPipelineStage<T> stage) : this("Test for: " + stage.Name, stage) { }
        public TestPipelineStage(string name, IPipelineStage<T> stage)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            this.AddDependencies(stage);
        }
        public string Name { get; }
        public IPipelineStage<T> Stage { get; }

        public List<T> RetrievedValues { get; } = new List<T>();
        public HashSet<IPipelineStage> InvalidatedStages { get; private set; } = new HashSet<IPipelineStage>();
        public int RetrievedValuesCount => RetrievedValues.Count;
        public long InvalidateDatum { get; private set; } = -1;

        public int Invalidations { get; private set; }

        public T GetValue()
        {
            var value = Stage.GetValue();
            RetrievedValues.Add(value);
            return value;
        }

        public void AssertStageInvalidated(IPipelineStage sut) => Assert.IsTrue(InvalidatedStages.Contains(sut), $"Stage {sut} was not invalidated when stage {Stage} was called.");
        public void AssertStageNotInvalidated(IPipelineStage sut) => Assert.IsFalse(InvalidatedStages.Contains(sut), $"Stage {sut} was invalidated when stage {Stage} was called.");
        public void AssertNotInvalidatedNorRetrieved() => AssertInvalidationsAndRetrievals(0, 0);
        public void AssertInvalidationsAndRetrievals(int invalidations, int retrievals)
        {
            AssertInvalidations(invalidations);
            AssertRetrievals(retrievals);
        }

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            InvalidateDatum = Interlocked.Increment(ref InvalidateDatumSource);
            Invalidations++;
            InvalidatedStages = new HashSet<IPipelineStage>(invalidator.AllInvalidatedStages);
            invalidator.InvalidateAllDependentStages(this);
        }

        public void AssertInvalidations(int invalidates) => Assert.AreEqual(invalidates, Invalidations, "Stage was not invalidated the expected number of times.");
        public void AssertRetrievals(int num) => Assert.AreEqual(num, RetrievedValuesCount, "Stage value was not retrieved the expected number of times.");
        public void AssertInvalidatedBefore(params TestPipelineStage<T>[] stages)
        {
            Assert.GreaterOrEqual(InvalidateDatum, 0, $"{Stage} has not been invalidated");
            foreach (var stage in stages)
                Assert.Less(InvalidateDatum, stage.InvalidateDatum, $"Stage {stage.Stage} was invalidated before stage {Stage}");
        }

        internal void PrepareForNext() => InvalidatedStages.Clear();


        public static void PrepareForNext(params TestPipelineStage<T>[] pipelines)
        {
            foreach (var pipeline in pipelines)
                pipeline.PrepareForNext();
        }
    }
}
