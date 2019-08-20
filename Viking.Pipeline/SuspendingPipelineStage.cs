using System;

namespace Viking.Pipeline
{
    public enum PipelineSuspending
    {
        Resume,
        ResumeWithoutPending,
        Suspend
    }

    public class SuspendingPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public SuspendingPipelineStage(IPipelineStage<TValue> input, IPipelineStage<PipelineSuspending> suspend)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Suspend = suspend ?? throw new ArgumentNullException(nameof(suspend));
            Name = "Suspender for: " + input.Name;
            this.AddDependencies(input, suspend);
        }

        public IPipelineStage<TValue> Input { get; }
        public IPipelineStage<PipelineSuspending> Suspend { get; }
        public string Name { get; }

        public PipelineSuspending SuspensionState => Suspend.GetValue();
        public bool HasPendingInvalidate { get; private set; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            var shouldInvalidate = HasPendingInvalidate;
            switch (SuspensionState)
            {
                case PipelineSuspending.Resume:
                    HasPendingInvalidate = false;
                    break;
                case PipelineSuspending.Suspend:
                    HasPendingInvalidate = true;
                    shouldInvalidate = false;
                    break;
                case PipelineSuspending.ResumeWithoutPending:
                    HasPendingInvalidate = false;
                    shouldInvalidate = false;
                    break;
            }

            if (shouldInvalidate)
                invalidator.InvalidateAllDependentStages(this);
            else
                invalidator.Revalidate(this);
        }
    }
}
