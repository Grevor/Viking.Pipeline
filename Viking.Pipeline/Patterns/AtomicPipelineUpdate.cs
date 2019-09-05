using System.Collections.Generic;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Enables atomic update of stages through a fluid interface.
    /// </summary>
    public sealed class AtomicPipelineUpdate
    {
        private HashSet<IPipelineStage> PendingStages { get; } = new HashSet<IPipelineStage>();

        /// <summary>
        /// Starts a new <see cref="AtomicPipelineUpdate"/>.
        /// </summary>
        public AtomicPipelineUpdate() { }

        /// <summary>
        /// Adds the specified stage to the update, without doing any updating.
        /// </summary>
        /// <param name="stage">The stage to add for the update.</param>
        /// <returns>The same update object.</returns>
        public AtomicPipelineUpdate Update(IPipelineStage stage)
        {
            PendingStages.Add(stage);
            return this;
        }

        /// <summary>
        /// Sets the value of the specified <see cref="AssignablePipelineStage{TValue}"/>, and adds it to the atomic update.
        /// </summary>
        /// <typeparam name="TValue">The data type.</typeparam>
        /// <param name="stage">The stage to set value for.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The same update object.</returns>
        public AtomicPipelineUpdate Update<TValue>(AssignablePipelineStage<TValue> stage, TValue value)
        {
            if (stage.SetValueWithoutInvalidating(value))
                PendingStages.Add(stage);
            return this;
        }

        /// <summary>
        /// Complete the update, invalidating all updated stages as an atomic operation.
        /// </summary>
        /// <returns>The same update object, now ready to start another atomic update.</returns>
        public AtomicPipelineUpdate Complete()
        {
            PipelineCore.Invalidate(PendingStages);
            PendingStages.Clear();
            return this;
        }
    }
}
