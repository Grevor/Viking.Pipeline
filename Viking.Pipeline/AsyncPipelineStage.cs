using System;
using System.Threading.Tasks;

namespace Viking.Pipeline
{
    public class AsyncPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public AsyncPipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Async invoker for: " + input.Name;
            this.AddDependencies(input);
        }

        public IPipelineStage<TValue> Input { get; }
        public string Name { get; }

        public TValue GetValue()
        {
            var result = new ResultCache();
            AsyncGet(result);
            return result.Value;
        }
        private async void AsyncGet(ResultCache cache) => cache.Value = await Task.Run(Input.GetValue);

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);


        private class ResultCache
        {
            public TValue Value { get; set; }
        }
    }
}
