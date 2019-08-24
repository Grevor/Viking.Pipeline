using System;

namespace Viking.Pipeline
{
    public enum PipelineSuspensionState
    {
        Resume,
        ResumeWithoutPendingInvalidates,
        Suspend
    }

    public class SuspendingPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public SuspendingPipelineStage(IPipelineStage<TValue> input, IPipelineStage<PipelineSuspensionState> suspend)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Suspend = suspend ?? throw new ArgumentNullException(nameof(suspend));
            Name = "Suspender for: " + input.Name;
            this.AddDependencies(input, suspend);
        }

        public IPipelineStage<TValue> Input { get; }
        public IPipelineStage<PipelineSuspensionState> Suspend { get; }
        public string Name { get; }

        public PipelineSuspensionState SuspensionState => Suspend.GetValue();
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
    }
}
