using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    /// <summary>
    /// Enables plugging in to the pipeline and reacting to changes.
    /// </summary>
    public sealed class ReactionPipelineStage : IPipelineStage
    {
        /// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified reaction being run if any of the specified stages are invalidated.
        /// </summary>
        /// <param name="reaction">The reaction to any invalidation.</param>
        /// <param name="stages">The stages to react to.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="reaction"/> or <paramref name="stages"/> are null.</exception>
        public ReactionPipelineStage(Action reaction, params IPipelineStage[] stages)
        {
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
            Stages = stages?.ToArray() ?? throw new ArgumentNullException(nameof(stages));
            Name = "Reaction to {" + string.Join(", ", Stages.Select(s => s.Name)) + "}";
            this.AddDependencies(stages);
        }

        public string Name { get; }
        /// <summary>
        /// Gets the action which will be run as a reaction to any invalidations.
        /// </summary>
        public Action Reaction { get; }
        /// <summary>
        /// Gets the stages which 
        /// </summary>
        public IEnumerable<IPipelineStage> Stages { get; }

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            invalidator.InvalidateAllDependentStages(this);
            Reaction();
        }

        public override string ToString() => Name;
    }

    public partial class ReactionPipelineStage<TInput1> : IPipelineStage<TInput1>
    {
        /// <summary>
        /// Gets the pass through value of this <see cref="ReactionPipelineStage{TInput1}"/>.
        /// </summary>
        /// <returns>The pass-through value.</returns>
        public TInput1 GetValue() => Input1.GetValue();
    }
}
