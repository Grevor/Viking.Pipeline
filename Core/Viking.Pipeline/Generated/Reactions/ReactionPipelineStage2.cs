using System;
using System.Linq;

namespace Viking.Pipeline
{
	/// <summary>
    /// Enables reactions to changes in pipeline values.
    /// </summary>
    /// <typeparam name="TInput1">The type of input number 1.</typeparam>
	/// <typeparam name="TInput2">The type of input number 2.</typeparam>
    public sealed partial class ReactionPipelineStage<TInput1, TInput2> : IPipelineStage
    {
		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified reaction to the specified inputs.
        /// </summary>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		public ReactionPipelineStage(
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2) :
			this(GetDefaultName(input1, input2), reaction, input1, input2) 
		{ }

		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified reaction to the specified inputs.
        /// </summary>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
		public ReactionPipelineStage(
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			bool reactImmediately) :
			this(GetDefaultName(input1, input2), reaction, input1, input2, reactImmediately) 
		{ }

		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified name and reaction to the specified inputs.
        /// </summary>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		public ReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2) :
			this(name, reaction, input1, input2, true) 
		{ }

		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified name and reaction to the specified inputs.
        /// </summary>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
		public ReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2, 
			bool reactImmediately)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
			this.AddDependencies(input1, input2);
			if (reactImmediately)
				Reaction(Input1.GetValue(), Input2.GetValue());
		}

		/// <summary>
        /// Gets the name of the reaction.
        /// </summary>
		public string Name { get; }
        /// <summary>
        /// The function which is called as a reaction to any change in inputs.
        /// </summary>
		public Action<TInput1, TInput2> Reaction { get; }
		/// <summary>
		/// Input number 1.
		/// </summary>
		public IPipelineStage<TInput1> Input1 { get; }
		/// <summary>
		/// Input number 2.
		/// </summary>
		public IPipelineStage<TInput2> Input2 { get; }

		/// <summary>
        /// Handles invalidation of the operation stage, and reacts as appropriate.
        /// </summary>
        /// <param name="invalidator">The invalidator.</param>
		public void OnInvalidate(IPipelineInvalidator invalidator)
		{
			invalidator.InvalidateAllDependentStages(this);
			Reaction(Input1.GetValue(), Input2.GetValue());
		}

		/// <summary>
        /// Gets a textual representation of this pipeline stage.
        /// </summary>
        /// <returns>A textual representation.</returns>
		public override string ToString() => $"{Name} - Reaction is {Reaction.GetDetailedStringRepresentation()}";

		private static string GetDefaultName(params IPipelineStage[] stages) => $"Reaction to {string.Join(", ", stages.Select(p => "'" + p.Name + "'"))}";
    }

	public static partial class PipelineReactions
	{
		/// <summary>
        /// Creates a reaction to the specified pipeline stages.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2>(
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2)
			=> new ReactionPipelineStage<TInput1, TInput2>(
				reaction,
				input1, input2);

		/// <summary>
        /// Creates a reaction to the specified pipeline stages.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2>(
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2>(
				reaction,
				input1, input2,
				reactImmediately);

		/// <summary>
        /// Creates a reaction to the specified pipeline stages.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2>(
			string name, 
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2)
			=> new ReactionPipelineStage<TInput1, TInput2>(
				name,
				reaction,
				input1, input2);

		/// <summary>
        /// Creates a reaction to the specified pipeline stages.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2>(
			string name, 
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2>(
				name,
				reaction,
				input1, input2,
				reactImmediately);
	}
}
