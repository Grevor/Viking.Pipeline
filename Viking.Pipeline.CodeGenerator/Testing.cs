using System;

namespace Viking.Pipeline.CodeGenerator
{
    class Testing
    {
        public static void Test()
        {
            var one = 1.AsPipelineConstant();
            var two = 2.AsPipelineConstant();

            var value = new AssignablePipelineStage<int>("integer", 0);
            var suspension = new AssignablePipelineStage<PipelineSuspensionState>("suspender", PipelineSuspensionState.Resume);
            var changeableOperation = new AssignablePipelineStage<Func<int, int>>("suspender", a => a);

            var multiply = PipelineOperations.Create("multiply", Multiply, value, two);
            var changeable = PipelineOperations.Create("changeable", changeableOperation, multiply);
            var cache = changeable.WithCache();
            var suspender = cache.WithConditionalSuspender(suspension);

            var reaction = PipelineReactions.Create(Reaction, suspender);

            value.SetValue(1);
            value.SetValue(2);
            changeableOperation.SetValue(a => a * 2);

            GC.KeepAlive(reaction);
            Console.ReadLine();
        }

        private static int Multiply(int a, int b) => a * b;

        private static void Reaction(int a) => Console.WriteLine("New value: " + a.ToString());
    }
}
