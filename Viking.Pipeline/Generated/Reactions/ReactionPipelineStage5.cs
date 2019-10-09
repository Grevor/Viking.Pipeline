using System;
using System.Linq;

namespace Viking.Pipeline
{
	/// <summary>
    /// Enables reactions to changes in pipeline values.
    /// </summary>
    /// <typeparam name="TInput1">The type of input number 1.</typeparam>
	/// <typeparam name="TInput2">The type of input number 2.</typeparam>
	/// <typeparam name="TInput3">The type of input number 3.</typeparam>
	/// <typeparam name="TInput4">The type of input number 4.</typeparam>
	/// <typeparam name="TInput5">The type of input number 5.</typeparam>
    public sealed partial class ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5> : IPipelineStage
    {
		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified reaction to the specified inputs.
        /// </summary>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
		public ReactionPipelineStage(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5) :
			this(GetDefaultName(input1, input2, input3, input4, input5), reaction, input1, input2, input3, input4, input5) 
		{ }

		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified reaction to the specified inputs.
        /// </summary>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
		public ReactionPipelineStage(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			bool reactImmediately) :
			this(GetDefaultName(input1, input2, input3, input4, input5), reaction, input1, input2, input3, input4, input5, reactImmediately) 
		{ }

		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified name and reaction to the specified inputs.
        /// </summary>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
		public ReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5) :
			this(name, reaction, input1, input2, input3, input4, input5, true) 
		{ }

		/// <summary>
        /// Creates a new <see cref="ReactionPipelineStage"/> with the specified name and reaction to the specified inputs.
        /// </summary>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
		public ReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5, 
			bool reactImmediately)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
			Input3 = input3 ?? throw new ArgumentNullException(nameof(input3));
			Input4 = input4 ?? throw new ArgumentNullException(nameof(input4));
			Input5 = input5 ?? throw new ArgumentNullException(nameof(input5));
			this.AddDependencies(input1, input2, input3, input4, input5);
			if (reactImmediately)
				Reaction(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue());
		}

		/// <summary>
        /// Gets the name of the reaction.
        /// </summary>
		public string Name { get; }
        /// <summary>
        /// The function which is called as a reaction to any change in inputs.
        /// </summary>
		public Action<TInput1, TInput2, TInput3, TInput4, TInput5> Reaction { get; }
		/// <summary>
		/// Input number 1.
		/// </summary>
		public IPipelineStage<TInput1> Input1 { get; }
		/// <summary>
		/// Input number 2.
		/// </summary>
		public IPipelineStage<TInput2> Input2 { get; }
		/// <summary>
		/// Input number 3.
		/// </summary>
		public IPipelineStage<TInput3> Input3 { get; }
		/// <summary>
		/// Input number 4.
		/// </summary>
		public IPipelineStage<TInput4> Input4 { get; }
		/// <summary>
		/// Input number 5.
		/// </summary>
		public IPipelineStage<TInput5> Input5 { get; }

		/// <summary>
        /// Handles invalidation of the operation stage, and reacts as appropriate.
        /// </summary>
        /// <param name="invalidator">The invalidator.</param>
		public void OnInvalidate(IPipelineInvalidator invalidator)
		{
			invalidator.InvalidateAllDependentStages(this);
			Reaction(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue());
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
		/// <typeparam name="TInput3">The type of input number 3.</typeparam>
		/// <typeparam name="TInput4">The type of input number 4.</typeparam>
		/// <typeparam name="TInput5">The type of input number 5.</typeparam>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				reaction,
				input1, input2, input3, input4, input5);

		/// <summary>
        /// Creates a reaction to the specified pipeline stages.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
		/// <typeparam name="TInput3">The type of input number 3.</typeparam>
		/// <typeparam name="TInput4">The type of input number 4.</typeparam>
		/// <typeparam name="TInput5">The type of input number 5.</typeparam>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				reaction,
				input1, input2, input3, input4, input5,
				reactImmediately);

		/// <summary>
        /// Creates a reaction to the specified pipeline stages.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
		/// <typeparam name="TInput3">The type of input number 3.</typeparam>
		/// <typeparam name="TInput4">The type of input number 4.</typeparam>
		/// <typeparam name="TInput5">The type of input number 5.</typeparam>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				name,
				reaction,
				input1, input2, input3, input4, input5);

		/// <summary>
        /// Creates a reaction to the specified pipeline stages.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
		/// <typeparam name="TInput3">The type of input number 3.</typeparam>
		/// <typeparam name="TInput4">The type of input number 4.</typeparam>
		/// <typeparam name="TInput5">The type of input number 5.</typeparam>
        /// <param name="name">The name of the new reaction.</param>
        /// <param name="reaction">The reaction delegate.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
        /// <param name="reactImmediately">Denotes if the reaction should fire immediately upon construction.</param>
        /// <returns>A reaction stage. The reaction is only guaranteed to fire while you hold on to this reference.</returns>
		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				name,
				reaction,
				input1, input2, input3, input4, input5,
				reactImmediately);
	}
}
