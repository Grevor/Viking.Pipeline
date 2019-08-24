using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    /// <summary>
    /// Extracts information about the propagation of the pipeline.
    /// </summary>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public sealed class PipelinePropagationInformationStage<TOutput> : IPipelineStage<TOutput>
    {
        /// <summary>
        /// Creates a new <see cref="PipelinePropagationInformationStage{TOutput}"/> with the specified name, information extractor and initial output.
        /// The new stage will listen to all the specified stages.
        /// </summary>
        /// <param name="name">The name of this stage.</param>
        /// <param name="extractor">The function used to extract information.</param>
        /// <param name="initial">The initial value.</param>
        /// <param name="stages">The stages to listen to.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/>, <paramref name="extractor"/> or <paramref name="stages"/> is null.</exception>
        public PipelinePropagationInformationStage(string name, Func<IPipelineInvalidator, TOutput> extractor, TOutput initial, params IPipelineStage[] stages)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            LastInvalidationInformation = initial;
            Stages = stages?.ToArray() ?? throw new ArgumentNullException(nameof(stages));
            this.AddDependencies(stages);
        }

        public string Name { get; }
        /// <summary>
        /// Gets the information extraction function.
        /// </summary>
        public Func<IPipelineInvalidator, TOutput> Extractor { get; }
        /// <summary>
        /// Gets the stages on which this stage is listening on.
        /// </summary>
        public IEnumerable<IPipelineStage> Stages { get; }

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
