using System;

namespace Viking.Pipeline
{
	/// <summary>
    /// Enables operations on pipeline values, yielding a new pipeline as a result.
    /// </summary>
    /// <typeparam name="TInput1">The type of input number 1.</typeparam>
	/// <typeparam name="TInput2">The type of input number 2.</typeparam>
	/// <typeparam name="TInput3">The type of input number 3.</typeparam>
	/// <typeparam name="TInput4">The type of input number 4.</typeparam>
	/// <typeparam name="TInput5">The type of input number 5.</typeparam>
	/// <typeparam name="TInput6">The type of input number 6.</typeparam>
	/// <typeparam name="TInput7">The type of input number 7.</typeparam>
	/// <typeparam name="TInput8">The type of input number 8.</typeparam>
    /// <typeparam name="TOutput">The stage output type.</typeparam>
    public sealed class OperationPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput> : IPipelineStage<TOutput>
    {
		/// <summary>
        /// Creates a new OperationPipelineStage with the specified name, operation and inputs.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
		/// <param name="input6">Input number 6.</param>
		/// <param name="input7">Input number 7.</param>
		/// <param name="input8">Input number 8.</param>
		public OperationPipelineStage(
			string name, 
			Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8) :
			this(name, operation?.AsPipelineConstant()!, input1, input2, input3, input4, input5, input6, input7, input8)
		{ }

		/// <summary>
        /// Creates a new OperationPipelineStage with the specified name, operation and inputs.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The pipeline from which to retrieve the operation.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
		/// <param name="input6">Input number 6.</param>
		/// <param name="input7">Input number 7.</param>
		/// <param name="input8">Input number 8.</param>
		public OperationPipelineStage(
			string name, 
			IPipelineStage<Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Operation = operation ?? throw new ArgumentNullException(nameof(operation));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
			Input3 = input3 ?? throw new ArgumentNullException(nameof(input3));
			Input4 = input4 ?? throw new ArgumentNullException(nameof(input4));
			Input5 = input5 ?? throw new ArgumentNullException(nameof(input5));
			Input6 = input6 ?? throw new ArgumentNullException(nameof(input6));
			Input7 = input7 ?? throw new ArgumentNullException(nameof(input7));
			Input8 = input8 ?? throw new ArgumentNullException(nameof(input8));
			this.AddDependencies(operation, input1, input2, input3, input4, input5, input6, input7, input8);
		}

		/// <summary>
        /// Gets the name of the operation.
        /// </summary>
		public string Name { get; }
		/// <summary>
		/// Gets the pipeline stage describing the operation to be performed.
		/// </summary>
		private IPipelineStage<Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>> Operation { get; }
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
		/// Input number 6.
		/// </summary>
		public IPipelineStage<TInput6> Input6 { get; }
		/// <summary>
		/// Input number 7.
		/// </summary>
		public IPipelineStage<TInput7> Input7 { get; }
		/// <summary>
		/// Input number 8.
		/// </summary>
		public IPipelineStage<TInput8> Input8 { get; }

		/// <summary>
        /// Gets the result of performing the operation on all specified inputs.
        /// </summary>
        /// <returns>The resulting value.</returns>
		public TOutput GetValue() => Operation.GetValue().Invoke(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue(), Input6.GetValue(), Input7.GetValue(), Input8.GetValue());

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
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
		/// <typeparam name="TInput3">The type of input number 3.</typeparam>
		/// <typeparam name="TInput4">The type of input number 4.</typeparam>
		/// <typeparam name="TInput5">The type of input number 5.</typeparam>
		/// <typeparam name="TInput6">The type of input number 6.</typeparam>
		/// <typeparam name="TInput7">The type of input number 7.</typeparam>
		/// <typeparam name="TInput8">The type of input number 8.</typeparam>
        /// <typeparam name="TOutput">The stage output type.</typeparam>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
		/// <param name="input6">Input number 6.</param>
		/// <param name="input7">Input number 7.</param>
		/// <param name="input8">Input number 8.</param>
        /// <returns>The pipeline stage reflecting the result of the operation.</returns>
		public static IPipelineStage<TOutput> Create<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>(
			string name, 
			Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8)
			=> new OperationPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>(
				name,
				operation,
				input1, input2, input3, input4, input5, input6, input7, input8);

		/// <summary>
        /// Create a new pipeline operation.
        /// </summary>
        /// <typeparam name="TInput1">The type of input number 1.</typeparam>
		/// <typeparam name="TInput2">The type of input number 2.</typeparam>
		/// <typeparam name="TInput3">The type of input number 3.</typeparam>
		/// <typeparam name="TInput4">The type of input number 4.</typeparam>
		/// <typeparam name="TInput5">The type of input number 5.</typeparam>
		/// <typeparam name="TInput6">The type of input number 6.</typeparam>
		/// <typeparam name="TInput7">The type of input number 7.</typeparam>
		/// <typeparam name="TInput8">The type of input number 8.</typeparam>
        /// <typeparam name="TOutput">The stage output type.</typeparam>
        /// <param name="name">The name of the operation.</param>
        /// <param name="operation">The pipeline from which to retrieve the operation.</param>
        /// <param name="input1">Input number 1.</param>
		/// <param name="input2">Input number 2.</param>
		/// <param name="input3">Input number 3.</param>
		/// <param name="input4">Input number 4.</param>
		/// <param name="input5">Input number 5.</param>
		/// <param name="input6">Input number 6.</param>
		/// <param name="input7">Input number 7.</param>
		/// <param name="input8">Input number 8.</param>
        /// <returns>The pipeline stage reflecting the result of the operation.</returns>
		public static IPipelineStage<TOutput> Create<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>(
			string name, 
			IPipelineStage<Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8)
			=> new OperationPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>(
				name,
				operation,
				input1, input2, input3, input4, input5, input6, input7, input8);
	}
}
