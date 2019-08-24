using System;
using System.Collections.Generic;

namespace Viking.Pipeline
{
    public class PipelineGraph
    {
        private Dictionary<IPipelineStage, PipelineGraphNode> Nodes { get; } = new Dictionary<IPipelineStage, PipelineGraphNode>();
        private List<PipelineGraphNode> TopologySorted { get; } = new List<PipelineGraphNode>();

        internal void AddNode(IPipelineStage s) => AddNode(s, true);
        private PipelineGraphNode AddNode(IPipelineStage stage, bool addAsTopology)
        {
            if (!Nodes.TryGetValue(stage, out var node))
            {
                node = new PipelineGraphNode(stage);
                Nodes.Add(stage, node);
            }

            if (addAsTopology)
                TopologySorted.Add(node);
            return node;
        }

        internal void AddEdge(IPipelineStage from, IPipelineStage to)
        {
            var fn = AddNode(from, false);
            var tn = AddNode(to, false);

            fn.AddDependentNode(tn);
        }
        internal void Invalidate() => IsValid = false;

        public PipelineGraphNode GetNode(IPipelineStage stage) => Nodes[stage];
        public bool HasNode(IPipelineStage stage) => Nodes.ContainsKey(stage);
        public IEnumerable<PipelineGraphNode> TopologySortedNodes => TopologySorted;
        public bool IsValid { get; private set; } = true;

    }

    public class PipelineGraphNode
    {
        public PipelineGraphNode(IPipelineStage stage) => Stage = stage ?? throw new ArgumentNullException(nameof(stage));

        private List<PipelineGraphNode> Nodes { get; } = new List<PipelineGraphNode>();

        public IPipelineStage Stage { get; }
        public string Name => Stage.Name;
        public IEnumerable<PipelineGraphNode> DependentNodes => Nodes;

        internal void AddDependentNode(PipelineGraphNode node) => Nodes.Add(node);
    }
}
