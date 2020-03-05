using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Defines the suspension stage of a <see cref="SuspendingPipelineStage{TValue}"/>.
    /// </summary>
    public enum PipelineSuspensionState
    {
        /// <summary>
        /// Resumes propagation, acting on any pending invalidations.
        /// </summary>
        Resume,
        /// <summary>
        /// Resumes propagation, but ignores and clears pending invalidations.
        /// </summary>
        ResumeWithoutPendingInvalidates,
        /// <summary>
        /// Suspends propagation, causing any invalidates to the input to be stored as a pending invalidate.
        /// </summary>
        Suspend
    }

    /// <summary>
    /// Controls further propagation of invalidates by allowing suspending and resuming.
    /// </summary>
    /// <typeparam name="TValue">The value passed through this stage.</typeparam>
    public sealed class SuspendingPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="SuspendingPipelineStage{TValue}"/> with the specified input and suspension input.
        /// </summary>
        /// <param name="input">The input to suspend.</param>
        /// <param name="suspend">The stage providing the current suspension state.</param>
        public SuspendingPipelineStage(IPipelineStage<TValue> input, IPipelineStage<PipelineSuspensionState> suspend)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Suspend = suspend ?? throw new ArgumentNullException(nameof(suspend));
            Name = "Suspender for: " + input.Name;
            this.AddDependencies(input, suspend);
        }

        /// <summary>
        /// The input to suspend propagation for.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }
        /// <summary>
        /// The suspension state stage.
        /// </summary>
        public IPipelineStage<PipelineSuspensionState> Suspend { get; }
        public string Name { get; }

        /// <summary>
        /// Gets the current <see cref="PipelineSuspensionState"/>.
        /// </summary>
        public PipelineSuspensionState SuspensionState => Suspend.GetValue();
        /// <summary>
        /// Checks if there are any pending invalidates on this stage.
        /// </summary>
        public bool HasPendingInvalidate { get; private set; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            var invalidateFromInput = invalidator.IsInvalidated(Input);
            var shouldInvalidate = HasPendingInvalidate || invalidateFromInput;
            switch (SuspensionState)
            {
                case PipelineSuspensionState.Resume:
                    HasPendingInvalidate = false;
                    break;
                case PipelineSuspensionState.Suspend:
                    HasPendingInvalidate = shouldInvalidate;
                    shouldInvalidate = false;
                    break;
                case PipelineSuspensionState.ResumeWithoutPendingInvalidates:
                    HasPendingInvalidate = false;
                    shouldInvalidate = invalidateFromInput;
                    break;
            }

            if (shouldInvalidate)
                invalidator.InvalidateAllDependentStages(this);
            else
                invalidator.Revalidate(this);
        }

        public override string ToString() => FormattableString.Invariant($"Suspender - State: {SuspensionState} Pending: {HasPendingInvalidate} Input: '{Input.Name}'");
    }
}
