using System;

namespace Viking.Pipeline
{
    public class OperationPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput> : IPipelineStage<TOutput>
    {
		public OperationPipelineStage(
			string name, 
			Func<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5) :
			this(name, operation?.AsPipelineConstant(), input1, input2, input3, input4, input5)
		{ }

		public OperationPipelineStage(
			string name, 
			IPipelineStage<Func<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput>> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Operation = operation ?? throw new ArgumentNullException(nameof(operation));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
			Input3 = input3 ?? throw new ArgumentNullException(nameof(input3));
			Input4 = input4 ?? throw new ArgumentNullException(nameof(input4));
			Input5 = input5 ?? throw new ArgumentNullException(nameof(input5));
			this.AddDependencies(operation, input1, input2, input3, input4, input5);
		}

		public string Name { get; }
		private IPipelineStage<Func<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput>> Operation { get; }
		public IPipelineStage<TInput1> Input1 { get; }
		public IPipelineStage<TInput2> Input2 { get; }
		public IPipelineStage<TInput3> Input3 { get; }
		public IPipelineStage<TInput4> Input4 { get; }
		public IPipelineStage<TInput5> Input5 { get; }

		public TOutput GetValue() => Operation.GetValue().Invoke(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue());

		public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

		public override string ToString() => $"{Name} - Operation is {Operation.GetValue()?.GetDetailedStringRepresentation() ?? "<null>" }";
    }

	public static partial class PipelineOperations
	{
		public static IPipelineStage<TOutput> Create<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput>(
			string name, 
			Func<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5)
			=> new OperationPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput>(
				name,
				operation,
				input1, input2, input3, input4, input5);

		public static IPipelineStage<TOutput> Create<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput>(
			string name, 
			IPipelineStage<Func<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput>> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5)
			=> new OperationPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TOutput>(
				name,
				operation,
				input1, input2, input3, input4, input5);
	}
}
