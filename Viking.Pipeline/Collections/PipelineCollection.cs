using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline.Collections
{


    public class PipelineCollection<TKey, TValue>
    {
        private Suspender Suspender { get; } = new Suspender();
        private bool IsSuspended => Suspender.IsSuspended;
        private Dictionary<TKey, ChangeEntry> ChangeSet { get; } = new Dictionary<TKey, ChangeEntry>();

        private AssignablePipelineStage<IEnumerable<PipelineCollectionEntry<TKey, TValue>>> AddedEntriesAssignable { get; }
        private AssignablePipelineStage<IEnumerable<PipelineCollectionEntry<TKey, TValue>>> ChangedEntriesAssignable { get; }
        private AssignablePipelineStage<IEnumerable<PipelineCollectionEntry<TKey, TValue>>> RemovedEntriesAssignable { get; }

        private Dictionary<TKey, Entry> Collection { get; }

        public TValue this[TKey key]
        {
            get => Collection[key].Value;
            set
            {
                if (Add(key, value))
                    return;

                if (!TryGetEntry(key, out var entry))
                    throw new InvalidOperationException("Faulty key equality or hashcode");

                if (!entry.Pipeline.SetValueWithoutInvalidating(value))
                    return;

                if (IsSuspended)
                {
                    var change = GetChangeEntry(key, entry.Pipeline, InitialState.Present);

                    if (change.InitialState == InitialState.Present)
                        change.State = ChangeState.Changed;
                    else
                        change.State = ChangeState.Added;
                }
                else
                {
                    ChangeSet.Add(key, CreateSingleEntry(entry, ChangeState.Changed));
                    ResumeAllUpdates();
                }
            }
        }

        public IPipelineStage<TValue> GetPipeline(TKey key) => Collection[key].Pipeline;
        private bool TryGetEntry(TKey key, out Entry entry) => Collection.TryGetValue(key, out entry);

        public bool Contains(TKey key) => Collection.ContainsKey(key);

        public bool Add(TKey key, TValue value)
        {
            if (Contains(key))
                return false;

            var entry = new Entry(key, new AssignablePipelineStage<TValue>("", value));
            Collection.Add(key, entry);

            if (IsSuspended)
            {
                var change = GetChangeEntry(key, entry.Pipeline, InitialState.NotPresent);
                if (change.InitialState == InitialState.NotPresent)
                    change.State = ChangeState.Added;
                else
                    change.State = ChangeState.Changed;
            }
            else
            {
                ChangeSet.Add(key, CreateSingleEntry(entry, ChangeState.Changed));
                ResumeAllUpdates();
            }
            return true;
        }
        public bool Remove(TKey key)
        {
            if (!TryGetEntry(key, out var entry))
                return false;
            Collection.Remove(key);

            if (IsSuspended)
            {
                var change = GetChangeEntry(key, entry.Pipeline, InitialState.Present);
                if (change.InitialState == InitialState.Present)
                    change.State = ChangeState.Removed;
                else if (change.InitialState == InitialState.NotPresent)
                    change.State = ChangeState.NoChange;
            }
            else
            {
                ChangeSet.Add(key, CreateSingleEntry(entry, ChangeState.Removed));
                ResumeAllUpdates();
            }
            return true;
        }

        private static ChangeEntry CreateSingleEntry(Entry entry, ChangeState state)
        {
            return new ChangeEntry(entry.Key, entry.Pipeline, InitialState.Present) { State = ChangeState.Removed };
        }

        private ChangeEntry GetChangeEntry(TKey key, IPipelineStage<TValue> pipeline, InitialState state)
        {
            if (!ChangeSet.TryGetValue(key, out var entry))
            {
                entry = new ChangeEntry(key, pipeline, state);
                ChangeSet.Add(key, entry);
            }
            return entry;
        }

        public void BeginUpdate() => Suspender.Suspend();
        public bool ResumeUpdate()
        {
            if (Suspender.Resume())
            {
                ResumeAllUpdates();
                return true;
            }
            return false;
        }

        private void ResumeAllUpdates()
        {
            SetValues(RemovedEntriesAssignable, ChangeSet.Values.Where(c => c.State == ChangeState.Removed), false);
            SetValues(AddedEntriesAssignable, ChangeSet.Values.Where(c => c.State == ChangeState.Added), false);
            var changed = ChangeSet.Values.Where(c => c.State == ChangeState.Changed).ToList();
            SetValues(ChangedEntriesAssignable, changed, false);

            ChangeSet.Clear();

            var stages = changed.Select(e => e.Pipeline).Cast<IPipelineStage>().Concat(new[] { RemovedEntriesAssignable, AddedEntriesAssignable, ChangedEntriesAssignable });
            PipelineCore.Invalidate(stages);
        }

        private void SetValues(
            AssignablePipelineStage<IEnumerable<PipelineCollectionEntry<TKey, TValue>>> assignable,
            IEnumerable<ChangeEntry> changes,
            bool invalidate)
        {
            var actualValues = changes.Select(c => Collection[c.Key].AsEntry()).ToList();

            if (invalidate)
                assignable.SetValue(actualValues);
            else
                assignable.SetValueWithoutInvalidating(actualValues);
        }

        private sealed class Entry
        {
            public Entry(TKey key, AssignablePipelineStage<TValue> pipeline)
            {
                Key = key;
                Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            }

            public TKey Key { get; }
            public AssignablePipelineStage<TValue> Pipeline { get; }
            public TValue Value => Pipeline.Value;
            public PipelineCollectionEntry<TKey, TValue> AsEntry() => new PipelineCollectionEntry<TKey, TValue>(Key, Pipeline);
        }

        private sealed class ChangeEntry
        {
            public ChangeEntry(TKey key, IPipelineStage<TValue> pipeline, InitialState initialState)
            {
                Key = key;
                Pipeline = pipeline;
                InitialState = initialState;
            }

            public TKey Key { get; }
            public IPipelineStage<TValue> Pipeline { get; }
            public InitialState InitialState { get; }
            public ChangeState State { get; set; }
        }

        private enum ChangeState
        {
            NoChange,
            Added,
            Removed,
            Changed
        }

        private enum InitialState
        {
            Present,
            NotPresent
        }
    }
}
