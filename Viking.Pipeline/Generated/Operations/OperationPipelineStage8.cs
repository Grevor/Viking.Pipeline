using System;

namespace Viking.Pipeline
{
    public class OperationPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput> : IPipelineStage<TOutput>
    {
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
			this(name, operation?.AsPipelineConstant(), input1, input2, input3, input4, input5, input6, input7, input8)
		{ }

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

		public string Name { get; }
		private IPipelineStage<Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8, TOutput>> Operation { get; }
		public IPipelineStage<TInput1> Input1 { get; }
		public IPipelineStage<TInput2> Input2 { get; }
		public IPipelineStage<TInput3> Input3 { get; }
		public IPipelineStage<TInput4> Input4 { get; }
		public IPipelineStage<TInput5> Input5 { get; }
		public IPipelineStage<TInput6> Input6 { get; }
		public IPipelineStage<TInput7> Input7 { get; }
		public IPipelineStage<TInput8> Input8 { get; }

		public TOutput GetValue() => Operation.GetValue().Invoke(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue(), Input6.GetValue(), Input7.GetValue(), Input8.GetValue());

		public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

		public override string ToString() => $"{Name} - Operation is {Operation.GetValue()?.GetDetailedStringRepresentation() ?? "<null>" }";
    }

	public static partial class PipelineOperations
	{
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
