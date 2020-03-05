using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viking.Pipeline
{
    /// <summary>
    /// Pipeline stage aggregating outputs from multiple stages into a single <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public sealed class CollectingPipelineStage<TOutput> : IPipelineStage<IEnumerable<TOutput>>
    {
        /// <summary>
        /// Creates a new <see cref="CollectingPipelineStage{TOutput}"/> with the specified name and initial inputs.
        /// </summary>
        /// <param name="name">The name of this stage.</param>
        /// <param name="inputs">The initial inputs.</param>
        public CollectingPipelineStage(string name, IEnumerable<IPipelineStage<TOutput>> inputs)
        {
            if (inputs is null)
                throw new ArgumentNullException(nameof(inputs));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Inputs = inputs.ToList();
            this.AddDependencies(Inputs.ToArray());
        }

        public string Name { get; }
        private List<IPipelineStage<TOutput>> Inputs { get; }
        /// <summary>
        /// Gets all current input <see cref="IPipelineStage{TOutput}"/>.
        /// </summary>
        public IEnumerable<IPipelineStage<TOutput>> CurrentInputs => Inputs;

        /// <summary>
        /// Removes the specified inputs from this stage.
        /// </summary>
        /// <param name="inputs">The inputs to remove.</param>
        public void RemoveInputs(IEnumerable<IPipelineStage<TOutput>> inputs)
        {
            var stagesToRemove = inputs.Intersect(Inputs).ToArray();
            foreach (var input in stagesToRemove)
                Inputs.Remove(input);
            this.RemoveDependencies(stagesToRemove);

            if (stagesToRemove.Length > 0)
                this.Invalidate();
        }

        /// <summary>
        /// Adds the specified inputs from this stage.
        /// </summary>
        /// <param name="inputs">The inputs to add.</param>
        public void AddInputs(IEnumerable<IPipelineStage<TOutput>> inputs)
        {
            var stagesToAdd = inputs.Except(Inputs).ToArray();
            Inputs.AddRange(stagesToAdd);
            this.AddDependencies(stagesToAdd);

            if (stagesToAdd.Length > 0)
                this.Invalidate();
        }

        public IEnumerable<TOutput> GetValue() => Inputs.Select(pipeline => pipeline.GetValue()).ToList();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);
    }
}
