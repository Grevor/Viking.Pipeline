using NUnit.Framework;
using System.Collections.Generic;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class ComparerTests
    {
        [TestCase(1, 1, false)]
        [TestCase(1, 1, true)]
        [TestCase(1, 10, false)]
        [TestCase(1, 10, true)]
        public void EqualityCheckComparerUsesTheSuppliedFunction(int a, int b, bool result)
        {
            var comparer = PipelineComparers.Equality<int>((x, y) => result);

            Assert.AreEqual(result, comparer.Equals(a, b));
        }



        [TestCase(new int[] { }, new int[] { }, true)]
        [TestCase(new int[] { }, new int[] { 1 }, false)]

        [TestCase(new[] { 1, 2, 3}, new[] { 1, 2, 3 }, true)]
        [TestCase(new[] { 1, 3, 2}, new[] { 1, 2, 3 }, false)]

        [TestCase(new[] { 1, 2, 3 }, new[] { 1, 3 }, false)]
        [TestCase(new[] { 1, 2, 3 }, new[] { 1, 2 }, false)]

        [TestCase(new[] { 1 }, new[] { 2 }, false)]
        [TestCase(new[] { 1, 2 }, new[] {  4, 5 }, false)]
        public void SequenceEqualityGetsExpectedResults(int[] a, int[] b, bool match)
        {
            var comparer = PipelineComparers.SequenceEqualityComparer(EqualityComparer<int>.Default);

            Assert.AreEqual(match, comparer.Equals(a, b));
            Assert.AreEqual(match, comparer.Equals(b, a));
        }



        [TestCase(new int[] { }, new int[] { }, true)]
        [TestCase(new int[] { }, new int[] { 1 }, false)]

        [TestCase(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, true)]
        [TestCase(new[] { 1, 3, 2 }, new[] { 1, 2, 3 }, true)]
        [TestCase(new[] { 2, 3, 1 }, new[] { 1, 2, 3 }, true)]
        [TestCase(new[] { 3, 2, 1 }, new[] { 1, 2, 3 }, true)]

        [TestCase(new[] { 1, 2, 3 }, new[] { 1, 3 }, false)]
        [TestCase(new[] { 1, 2, 3 }, new[] { 1, 2 }, false)]

        [TestCase(new[] { 1 }, new[] { 2 }, false)]
        [TestCase(new[] { 1, 2 }, new[] { 4, 5 }, false)]
        public void SequenceEqualityWithoutOrderGetsExpectedResults(int[] a, int[] b, bool match)
        {
            var comparer = PipelineComparers.IgnoreOrderSequenceEqualityComparer(EqualityComparer<int>.Default);

            Assert.AreEqual(match, comparer.Equals(a, b));
            Assert.AreEqual(match, comparer.Equals(b, a));
        }
    }
}
