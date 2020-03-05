using System.Collections.Generic;

namespace Viking.Pipeline.Tests
{
    internal class SettableEqualityComparer<T> : IEqualityComparer<T>
    {
        public SettableEqualityComparer(bool equal)
        {
            Equal = equal;
        }

        public bool Equal { get; set; }

        public bool Equals(T x, T y) => Equal;

        public int GetHashCode(T obj) => 0;
    }
}
