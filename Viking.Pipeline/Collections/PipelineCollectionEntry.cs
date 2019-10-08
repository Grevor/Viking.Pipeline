using System;
using System.Collections.Generic;

namespace Viking.Pipeline.Collections
{
    public struct PipelineCollectionEntry<TKey, TValue> : IEquatable<PipelineCollectionEntry<TKey, TValue>>
    {
        public PipelineCollectionEntry(TKey key, IPipelineStage<TValue> pipeline)
        {
            Key = key;
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        public TKey Key { get; }
        public IPipelineStage<TValue> Pipeline { get; }
        public TValue Value => Pipeline.GetValue();

        public override bool Equals(object obj) => obj is PipelineCollectionEntry<TKey, TValue> entry && Equals(entry);
        public bool Equals(PipelineCollectionEntry<TKey, TValue> other)
        {
            return EqualityComparer<TKey>.Default.Equals(Key, other.Key) &&
                   EqualityComparer<IPipelineStage<TValue>>.Default.Equals(Pipeline, other.Pipeline);
        }

        public override int GetHashCode()
        {
            var hashCode = -542067313;
            hashCode = hashCode * -1521134295 + EqualityComparer<TKey>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<IPipelineStage<TValue>>.Default.GetHashCode(Pipeline);
            return hashCode;
        }

        public static bool operator ==(PipelineCollectionEntry<TKey, TValue> left, PipelineCollectionEntry<TKey, TValue> right) => left.Equals(right);
        public static bool operator !=(PipelineCollectionEntry<TKey, TValue> left, PipelineCollectionEntry<TKey, TValue> right) => !(left == right);
    }
}
