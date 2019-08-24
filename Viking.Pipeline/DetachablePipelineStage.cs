using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Enables quick detachment of anything which depends on this pipeline stage.
    /// </summary>
    /// <typeparam name="TValue">The value.</typeparam>
    public sealed class DetachablePipelineStage<TValue> : PassThroughPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="DetachablePipelineStage{TValue}"/> with the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is null.</exception>
        public DetachablePipelineStage(IPipelineStage<TValue> input) : base("Detacher for: " + (input?.Name ?? ""), input)
        {
        }

        /// <summary>
        /// Detach all dependent stages from this stage.
        /// </summary>
        public void DetachFromPipeline() => this.UnlinkAllDependencies();

        public override string ToString() => FormattableString.Invariant($"Detachable Stage - {Name}");
    }
}
