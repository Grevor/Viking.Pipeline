using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    /// <summary>
    /// Provides functions to create comparers.
    /// </summary>
    public static class PipelineComparers
    {

        /// <summary>
        /// Creates an <see cref="IEqualityComparer{T}"/> from a function. 
        /// Note that this comparer will retrieve hashcodes through the <see cref="T.GetHashCode()"/> method.
        /// </summary>
        /// <typeparam name="T">The type of object to check equality for.</typeparam>
        /// <param name="equalityCheck">The compare function to use.</param>
        /// <returns>An <see cref="IEqualityComparer{T}"/> created from the specified function. Hashcode is still retrieved through <code>x.GetHashCode()</code>.</returns>
        public static IEqualityComparer<T> Equality<T>(EqualityCheck<T> equalityCheck) => new EqualityOnlyComparer<T>(equalityCheck);
        /// <summary>
        /// Gets an equality comparer which checks if two sequences has the same items in the same order.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="elementComparer">The element comparer.</param>
        /// <returns>The <see cref="IEqualityComparer{T}"/>.</returns>
        public static IEqualityComparer<IEnumerable<T>> SequenceEqualityComparer<T>(IEqualityComparer<T> elementComparer) => new EnumerableEqualityComparer<T>(elementComparer);
        /// <summary>
        /// Gets an equality comparer which checks if two sequences has the same items, but in any order.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="elementComparer">The element comparer.</param>
        /// <returns>The <see cref="IEqualityComparer{T}"/>.</returns>
        public static IEqualityComparer<IEnumerable<T>> IgnoreOrderSequenceEqualityComparer<T>(IEqualityComparer<T> elementComparer) => new IgnoreOrderEnumerableEqualityComparer<T>(elementComparer);






        private class EqualityOnlyComparer<T> : IEqualityComparer<T>
        {
            public EqualityOnlyComparer(EqualityCheck<T> equalityCheck) => EqualityCheck = equalityCheck ?? throw new ArgumentNullException(nameof(equalityCheck));

            public EqualityCheck<T> EqualityCheck { get; }

            public bool Equals(T x, T y) => EqualityCheck.Invoke(x, y);
            public int GetHashCode(T obj) => obj.GetHashCode();
        }
        private class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
        {
            public EnumerableEqualityComparer(IEqualityComparer<T> elementComparer) => ElementComparer = elementComparer ?? throw new ArgumentNullException(nameof(elementComparer));

            public IEqualityComparer<T> ElementComparer { get; }

            public bool Equals(IEnumerable<T> x, IEnumerable<T> y) => x.SequenceEqual(y, ElementComparer);

            public int GetHashCode(IEnumerable<T> obj) => obj.GetHashCode();
        }

        private class IgnoreOrderEnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
        {
            public IgnoreOrderEnumerableEqualityComparer(IEqualityComparer<T> elementComparer) => ElementComparer = elementComparer ?? throw new ArgumentNullException(nameof(elementComparer));

            public IEqualityComparer<T> ElementComparer { get; }

            public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
            {
                var xHash = new HashSet<T>(x, ElementComparer);

                var ynum = 0;
                var yenum = y.GetEnumerator();
                var xnum = xHash.Count;

                while (yenum.MoveNext() && ++ynum <= xnum)
                {
                    if (!xHash.Contains(yenum.Current))
                        return false;
                }
                return ynum == xnum;
            }

            public int GetHashCode(IEnumerable<T> obj) => obj.GetHashCode();
        }
    }
}
