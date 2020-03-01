using NUnit.Framework;
using System.Linq;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class PipelineGraphTests
    {
        [Test]
        public void OnlyMinimalGraphIsRetrieved()
        {
            var a = 1.AsPipelineConstant();
            var b = 1.AsPipelineConstant();

            var c = PipelineOperations.Create("", (x, y) => x + y, a, b);
            var test = c.AttachTestStage();

            var graph = PipelineCore.GetPipelineGraphIncludingStages(new[] { b });
            Assert.AreEqual(3, graph.TopologySortedNodes.Count());
            AssertTopologyPosition(graph, 0, b);
            AssertTopologyPosition(graph, 1, c);
            AssertTopologyPosition(graph, 2, test);

            AssertNodeDependents(graph, b, c);
            AssertNodeDependents(graph, c, test);
        }

        [Test]
        public void GraphContainsNoDuplicatesWhenMultipleStagesAreSpecifiedAsContained()
        {
            var a = 1.AsPipelineConstant();
            var b = 1.AsPipelineConstant();

            var c = PipelineOperations.Create("", (x, y) => x + y, a, b);
            var test = c.AttachTestStage();

            var graph = PipelineCore.GetPipelineGraphIncludingStages(new[] { a, b });
            Assert.AreEqual(4, graph.TopologySortedNodes.Count());

            AssertNodeDependents(graph, a, c);
            AssertNodeDependents(graph, b, c);
            AssertNodeDependents(graph, c, test);
            AssertNodeDependents(graph, test);
        }

        [Test(Description = "Regression test for a propagation bug which could cause the wrong pipeline stage to be triggered when two hash codes overlapped.")]
        public void GraphDoesNotBreakDownIfTwoStagesHaveTheSameHashCode()
        {
            var constantA = 10.AsPipelineConstant();
            var constantB = 10.AsPipelineConstant();
            var a = new SameHashPipelineStage<int>(constantA);
            var b = new SameHashPipelineStage<int>(constantB);

            var testA = a.AttachTestStage();
            var testB = b.AttachTestStage();

            constantA.Invalidate();

            testA.AssertInvalidations(1);
            testB.AssertInvalidations(0);
        }


        private static void AssertTopologyPosition(PipelineGraph graph, int topologyIndex, IPipelineStage expected)
        {
            var node = graph.TopologySortedNodes.Skip(topologyIndex).First();
            Assert.AreEqual(expected, node.Stage);
        }

        private static void AssertNodeDependents(PipelineGraph graph, IPipelineStage stage, params IPipelineStage[] expected)
        {
            var node = graph.GetNode(stage);
            CollectionAssert.AreEquivalent(expected, node.DependentNodes.Select(n => n.Stage));
        }



        private class SameHashPipelineStage<T> : IPipelineStage<T>
        {
            public SameHashPipelineStage(IPipelineStage<T> input)
            {
                Input = input ?? throw new System.ArgumentNullException(nameof(input));
                this.AddDependencies(input);
            }

            public string Name => "Name";

            public IPipelineStage<T> Input { get; }

            public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

            public override int GetHashCode() => 0;

            public T GetValue() => Input.GetValue();
        }
    }
}
