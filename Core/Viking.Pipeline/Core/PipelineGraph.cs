using System;
using System.Collections.Generic;

namespace Viking.Pipeline
{
    /// <summary>
    /// Describes a part of a pipeline as a graph.
    /// </summary>
    public sealed class PipelineGraph
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

        /// <summary>
        /// Gets a node from the graph.
        /// </summary>
        /// <param name="stage">The stage of which to get a node from.</param>
        /// <returns>The node corresponding to the specified stage.</returns>
        public PipelineGraphNode GetNode(IPipelineStage stage) => Nodes[stage];
        /// <summary>
        /// Checks if the specified stage has a node in this <see cref="PipelineGraph"/>.
        /// </summary>
        /// <param name="stage">The stage to check for.</param>
        /// <returns>True if there is a node in the graph corresponding to the specified stage.</returns>
        public bool HasNode(IPipelineStage stage) => Nodes.ContainsKey(stage);
        /// <summary>
        /// Gets all nodes in this graph, in some topological order.
        /// </summary>
        public IEnumerable<PipelineGraphNode> TopologySortedNodes => TopologySorted;
    }

    /// <summary>
    /// A node in a <see cref="PipelineGraph"/>.
    /// </summary>
    public sealed class PipelineGraphNode
    {
        /// <summary>
        /// Creates a new pipeline graph node.
        /// </summary>
        /// <param name="stage"></param>
        public PipelineGraphNode(IPipelineStage stage) => Stage = stage ?? throw new ArgumentNullException(nameof(stage));

        private List<PipelineGraphNode> Nodes { get; } = new List<PipelineGraphNode>();

        /// <summary>
        /// The stage this node represents.
        /// </summary>
        public IPipelineStage Stage { get; }
        /// <summary>
        /// Gets the name of this node (the stage).
        /// </summary>
        public string Name => Stage.Name;
        /// <summary>
        /// Gets all nodes which are dependent on this node.
        /// </summary>
        public IEnumerable<PipelineGraphNode> DependentNodes => Nodes;

        internal void AddDependentNode(PipelineGraphNode node) => Nodes.Add(node);
    }
}
