using System;

namespace Viking.Pipeline.Collections
{
    public struct PipelineCollectionEntry<TKey, TValue>
    {
        public PipelineCollectionEntry(TKey key, IPipelineStage<TValue> pipeline)
        {
            Key = key;
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        public TKey Key { get; }
        public IPipelineStage<TValue> Pipeline { get; }
        public TValue Value => Pipeline.GetValue();
    }
}
