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
            Assert.IsTrue(graph.IsValid);
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
            Assert.IsTrue(graph.IsValid);
            Assert.AreEqual(4, graph.TopologySortedNodes.Count());

            AssertNodeDependents(graph, a, c);
            AssertNodeDependents(graph, b, c);
            AssertNodeDependents(graph, c, test);
            AssertNodeDependents(graph, test);
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
    }
}
