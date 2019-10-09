using System;

namespace Viking.Pipeline
{
	/// <summary>
    /// Enables operations on pipeline values, yielding a new pipeline as a result.
    /// </summary>
    /// <typeparam name="TInput1">The type of input number 1.</typeparam>
    /// <typeparam name="TOutput">The stage output type.</typeparam>
    public sealed class OperationPipelineStage<TInput1, TOutput> : IPipelineStage<TOutput>
    {
		/// <summary>
        /// Creates a new OperationPipelineStage with the specified name, operation and inputs.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="input1">Input number 1.</param>
		public OperationPipelineStage(
			string name, 
			Func<TInput1, TOutput> operation, 
			IPipelineStage<TInput1> input1) :
			this(name, operation?.AsPipelineConstant()!, input1)
		{ }

		/// <summary>
        /// Creates a new OperationPipelineStage with the specified name, operation and inputs.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The pipeline from which to retrieve the operation.</param>
        /// <param name="input1">Input number 1.</param>
		public OperationPipelineStage(
			string name, 
			IPipelineStage<Func<TInput1, TOutput>> operation, 
			IPipelineStage<TInput1> input1)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Operation = operation ?? throw new ArgumentNullException(nameof(operation));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			this.AddDependencies(operation, input1);
		}

		/// <summary>
        /// Gets the name of the operation.
        /// </summary>
		public string Name { get; }
		/// <summary>
		/// Gets the pipeline stage describing the operation to be performed.
		/// </summary>
		private IPipelineStage<Func<TInput1, TOutput>> Operation { get; }
		/// <summary>
		/// Input number 1.
		/// </summary>
		public IPipelineStage<TInput1> Input1 { get; }

		/// <summary>
        /// Gets the result of performing the operation on all specified inputs.
        /// </summary>
        /// <returns>The resulting value.</returns>
		public TOutput GetValue() => Operation.GetValue().Invoke(Input1.GetValue());

		/// <summary>
        /// Handles invalidation of the operation stage.
        /// </summary>
        /// <param name="invalidator">The invalidator.</param>
		public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

		/// <summary>
        /// Gets a textual representation of this pipeline stage.
        /// </summary>
        /// <returns>A textual representation.</returns>
		public override string ToString() => $"{Name} - Operation is {Operation.GetValue()?.GetDetailedStringRepresentation() ?? "<null>" }";
    }

	public static partial class PipelineOperations
	{
		/// <summary>
        /// Create a new pipeline operation.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
        /// <typeparam name="TOutput">The stage output type.</typeparam>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="input1">Input number 1.</param>
        /// <returns>The pipeline stage reflecting the result of the operation.</returns>
		public static IPipelineStage<TOutput> Create<TInput1, TOutput>(
			string name, 
			Func<TInput1, TOutput> operation, 
			IPipelineStage<TInput1> input1)
			=> new OperationPipelineStage<TInput1, TOutput>(
				name,
				operation,
				input1);

		/// <summary>
        /// Create a new pipeline operation.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
        /// <typeparam name="TOutput">The stage output type.</typeparam>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The pipeline from which to retrieve the operation.</param>
        /// <param name="input1">Input number 1.</param>
        /// <returns>The pipeline stage reflecting the result of the operation.</returns>
		public static IPipelineStage<TOutput> Create<TInput1, TOutput>(
			string name, 
			IPipelineStage<Func<TInput1, TOutput>> operation, 
			IPipelineStage<TInput1> input1)
			=> new OperationPipelineStage<TInput1, TOutput>(
				name,
				operation,
				input1);
	}
}
