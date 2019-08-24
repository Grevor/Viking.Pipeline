using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Pipeline stage which extracts information about the propagation of the pipeline.
    /// </summary>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public class PipelinePropagationInformationStage<TOutput> : IPipelineStage<TOutput>
    {
        public PipelinePropagationInformationStage(string name, Func<IPipelineInvalidator, TOutput> extractor, TOutput initial, params IPipelineStage[] stages)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            LastInvalidationInformation = initial;
            Stages = stages ?? throw new ArgumentNullException(nameof(stages));
            this.AddDependencies(stages);
        }

        public string Name { get; }
        public Func<IPipelineInvalidator, TOutput> Extractor { get; }
        public IPipelineStage[] Stages { get; }

        private TOutput LastInvalidationInformation { get; set; }

        public TOutput GetValue() => LastInvalidationInformation;

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            LastInvalidationInformation = Extractor(invalidator);
            invalidator.InvalidateAllDependentStages(this);
        }

        public override string ToString() => FormattableString.Invariant($"Pipeline Propagation Exctraction - {Name}");
    }
}
