using System;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Describes the expected hierarchical propagation behavior of a <see cref="HierarchicalSuspenderNode"/>.
    /// </summary>
    public enum HierarchicalBehavior
    {
        /// <summary>
        /// Propagate suspension down to children. Any resume state simply takes the childs state instead.
        /// </summary>
        PropagateSuspendOnly,
        /// <summary>
        /// The childs state cannot be "stronger" that the parent's state. 
        /// <see cref="PipelineSuspensionState.Resume"/> > <see cref="PipelineSuspensionState.ResumeWithoutPendingInvalidates"/> > <see cref="PipelineSuspensionState.Suspend"/>
        /// </summary>
        WeakenSuspensionState
    }

    /// <summary>
    /// Creates a hierarchy of suspenders
    /// </summary>
    public sealed class HierarchicalSuspenderNode
    {
        /// <summary>
        /// Creates a new <see cref="HierarchicalSuspenderNode"/> with the specified input as initial input.
        /// </summary>
        /// <param name="input"></param>
        public HierarchicalSuspenderNode(IPipelineStage<PipelineSuspensionState> input)
        {
            Output = input;
        }

        internal HierarchicalSuspenderNode(IPipelineStage<PipelineSuspensionState> parent, IPipelineStage<PipelineSuspensionState> input, HierarchicalBehavior behavior)
        {
            if (behavior == HierarchicalBehavior.WeakenSuspensionState)
                Output = PipelineOperations.Create("Hierarchical fusion (weaken suspension)", GetWeakestState, parent, input);
            else
                Output = PipelineOperations.Create("Hierarchical fusion (propagate suspend)", PropagateSuspensionOnly, parent, input);
        }

        /// <summary>
        /// Gets the output pipeline of the hierarchy.
        /// </summary>
        public IPipelineStage<PipelineSuspensionState> Output { get; }

        /// <summary>
        /// Creates a child to this hierarchy node.
        /// </summary>
        /// <param name="input">The input suspension state of the child.</param>
        /// <param name="behavior">The desired behavior.</param>
        /// <returns>The child hierarchy.</returns>
        public HierarchicalSuspenderNode CreateChild(IPipelineStage<PipelineSuspensionState> input, HierarchicalBehavior behavior)
        {
            return new HierarchicalSuspenderNode(Output, input, behavior);
        }

        /// <summary>
        /// Adds the suspender.
        /// </summary>
        /// <typeparam name="TValue">The value type of the stage.</typeparam>
        /// <param name="input">The stage.</param>
        /// <returns>The suspended stage.</returns>
        public IPipelineStage<TValue> WithSuspender<TValue>(IPipelineStage<TValue> input) => new SuspendingPipelineStage<TValue>(input, Output);

        /// <summary>
        /// Gets the weakest state of two.
        /// </summary>
        /// <param name="parent">The parent state.</param>
        /// <param name="child">The child state.</param>
        /// <returns>The weakest state, in the order Resume > ResumeWithoutInvalidate > Suspend.</returns>
        public static PipelineSuspensionState GetWeakestState(PipelineSuspensionState parent, PipelineSuspensionState child)
        {
            return parent switch
            {
                PipelineSuspensionState.Resume => child,
                PipelineSuspensionState.ResumeWithoutPendingInvalidates => child == PipelineSuspensionState.Resume ? PipelineSuspensionState.ResumeWithoutPendingInvalidates : child,
                PipelineSuspensionState.Suspend => PipelineSuspensionState.Suspend,
                _ => throw new ArgumentException("Invalid argument.", nameof(parent)),
            };
        }

        /// <summary>
        /// Gets the child state, unless parent is suspended.
        /// </summary>
        /// <param name="parent">The parent state.</param>
        /// <param name="child">The child state.</param>
        /// <returns><see cref="PipelineSuspensionState.Suspend"/> if parent is suspended, otherwise the child state.</returns>
        public static PipelineSuspensionState PropagateSuspensionOnly(PipelineSuspensionState parent, PipelineSuspensionState child)
        {
            if (parent == PipelineSuspensionState.Suspend)
                return PipelineSuspensionState.Suspend;
            return child;
        }
    }
}
