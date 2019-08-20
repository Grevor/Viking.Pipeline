using System;

namespace Viking.Pipeline
{
    public class CachingPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public CachingPipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Cache for: " + input.Name;
            this.AddDependencies(input);
        }

        public string Name { get; }
        public IPipelineStage<TValue> Input { get; }
        public bool IsValid { get; private set; }
        private TValue Cached { get; set; }

        public TValue GetValue()
        {
            if (!IsValid)
            {
                Cached = Input.GetValue();
                IsValid = true;
            }
            return Cached;
        }

        public void InvalidateCache()
        {
            IsValid = false;
            this.Invalidate();
        }

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            IsValid = false;
            invalidator.InvalidateAllDependentStages(this);
        }
    }
}
