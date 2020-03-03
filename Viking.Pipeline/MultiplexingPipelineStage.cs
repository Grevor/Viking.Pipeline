using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    /// <summary>
    /// Selects one of multiple input signals to output.
    /// </summary>
    /// <typeparam name="TSelect">The select signal type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public class MultiplexingPipelineStage<TSelect, TOutput> : IPipelineStage<TOutput>
    {
        /// <summary>
        /// Creates a new <see cref="MultiplexingPipelineStage{TSelect, TOutput}"/> with the given select signal, inputs and select signal comparer.
        /// </summary>
        /// <param name="name">The name of this pipeline stage.</param>
        /// <param name="select">The select signal.</param>
        /// <param name="inputs">The input stages.</param>
        public MultiplexingPipelineStage(
            string name,
            IPipelineStage<TSelect> select,
            IDictionary<TSelect, IPipelineStage<TOutput>> inputs)
            : this(name, select, inputs, EqualityComparer<TSelect>.Default)
        { }

        /// <summary>
        /// Creates a new <see cref="MultiplexingPipelineStage{TSelect, TOutput}"/> with the given select signal, inputs and select signal comparer.
        /// </summary>
        /// <param name="name">The name of this pipeline stage.</param>
        /// <param name="select">The select signal.</param>
        /// <param name="inputs">The input stages.</param>
        /// <param name="comparer">The comparer used when comparing the select signals.</param>
        public MultiplexingPipelineStage(
            string name, 
            IPipelineStage<TSelect> select, 
            IDictionary<TSelect, IPipelineStage<TOutput>> inputs, 
            IEqualityComparer<TSelect> comparer)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Select = select ?? throw new ArgumentNullException(nameof(select));
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            Inputs = new Dictionary<TSelect, IPipelineStage<TOutput>>(inputs, Comparer);

            this.AddDependencies(Select);
            this.AddDependencies(inputs.Values.ToArray());
        }

        public string Name { get; }
        private IPipelineStage<TSelect> Select { get; }
        private IEqualityComparer<TSelect> Comparer { get; }
        private Dictionary<TSelect, IPipelineStage<TOutput>> Inputs { get; }

        private TSelect CurrentSelectSignal => Select.GetValue();

        private bool HasInput(IPipelineStage<TOutput> stage) => Inputs.Values.Contains(stage);
        private IPipelineStage<TOutput> GetSelectedOutput()
        {
            return Inputs.TryGetValue(CurrentSelectSignal, out var selected)
            ? selected
            : throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Did not find input to multiplex for select signal {CurrentSelectSignal}"));
        }

        private void Remove(TSelect key)
        {
            if (Inputs.TryGetValue(key, out var input))
            {
                Inputs.Remove(key);
                if (!HasInput(input))
                    this.RemoveDependencies(input);
            }
        }

        /// <summary>
        /// Adds or changes an input mapping.
        /// </summary>
        /// <param name="key">The key to associate with the specified input stage.</param>
        /// <param name="input">The input stage.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is null.</exception>
        public void SetInput(TSelect key, IPipelineStage<TOutput> input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            Remove(key);

            var inputAlreadyAddedAsDependency = HasInput(input);
            Inputs.Add(key, input);

            if (!inputAlreadyAddedAsDependency)
                this.AddDependencies(input);

            if (Comparer.Equals(key, CurrentSelectSignal))
                this.Invalidate();
        }

        /// <summary>
        /// Removes an input mapping.
        /// </summary>
        /// <param name="key">The key to remove mapping for.</param>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is currently the selected input.</exception>
        public void RemoveInput(TSelect key)
        {
            if (Comparer.Equals(key, CurrentSelectSignal))
                throw new ArgumentException("Cannot remove the currently selected input.");

            Remove(key);
        }

        public TOutput GetValue() => GetSelectedOutput().GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            var selfInvalidated = !invalidator.AllInvalidatedStages.Skip(1).Any() && invalidator.IsInvalidated(this);

            var selectedInput = GetSelectedOutput();

            if (invalidator.IsInvalidated(Select) || invalidator.IsInvalidated(selectedInput) || selfInvalidated)
                invalidator.InvalidateAllDependentStages(this);
            else
                invalidator.Revalidate(this);
        }
    }
}
