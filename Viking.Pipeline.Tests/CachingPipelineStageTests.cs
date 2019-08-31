using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class CachingPipelineStageTests
    {
        [Test]
        public void InputIsAddedAsDependency()
        {
            var input = 10.AsPipelineConstant();
            var sut = CreateCache(input);

            PipelineAssert.DependentOn(sut, input);
        }

        [Test]
        public void ExceptionOnNullInputToConstructor() => PipelineAssert.NullArgumentException(() => new CachingPipelineStage<int>(null), "input");

        [Test]
        public void CacheStartsAsInvalid()
        {
            var assignable = GetAssignable(1);
            var cache = CreateCache(assignable);

            Assert.IsFalse(cache.IsValid);
        }

        [Test]
        public void CacheRetrievesValueWhenInvalid()
        {
            var assignable = GetAssignable(1);
            var cache = CreateCache(assignable);

            Assert.IsFalse(cache.IsValid);
            PipelineAssert.Value(cache, 1);
            Assert.IsTrue(cache.IsValid);
        }

        [Test]
        public void CacheInvalidateInvalidatesCache()
        {
            var assignable = GetAssignable(1);
            var cache = CreateCache(assignable);

            Assert.IsFalse(cache.IsValid);
            PipelineAssert.Value(cache, 1);
            Assert.IsTrue(cache.IsValid);
            cache.InvalidateCache();
            Assert.IsFalse(cache.IsValid);
        }

        [Test]
        public void CacheDoesNotRetrieveValueWhileValid()
        {
            var assignable = GetAssignable(1);
            var test = assignable.AttachTestStage();
            var cache = CreateCache(test);

            
            Assert.IsFalse(cache.IsValid);

            PipelineAssert.Value(cache, 1);
            Assert.IsTrue(cache.IsValid);
            test.AssertRetrievals(1);

            PipelineAssert.Value(cache, 1);
            Assert.IsTrue(cache.IsValid);
            test.AssertRetrievals(1);
        }

        [Test]
        public void CacheIsInvalidatedOnInvalidation()
        {
            var assignable = GetAssignable(1);
            var test = assignable.AttachTestStage();
            var cache = CreateCache(test);

            Assert.IsFalse(cache.IsValid);

            PipelineAssert.Value(cache, 1);
            Assert.IsTrue(cache.IsValid);
            test.AssertRetrievals(1);

            cache.Invalidate();
            Assert.IsFalse(cache.IsValid);

            PipelineAssert.Value(cache, 1);
            Assert.IsTrue(cache.IsValid);
            test.AssertRetrievals(2);
        }

        [Test]
        public void CacheRetrievesNewValueFromPreviousStageWhenItIsInvalidated()
        {
            var assignable = GetAssignable(1);
            var cache = CreateCache(assignable);

            PipelineAssert.Value(cache, 1);
            Assert.IsTrue(cache.IsValid);

            assignable.SetValue(2);
            Assert.IsFalse(cache.IsValid);
            PipelineAssert.Value(cache, 2);
        }

        [Test]
        public void CacheInvalidateSendsInvalidationToPipelineDependencies()
        {
            var cache = CreateCache(1.AsPipelineConstant());
            PipelineAssert.Value(cache, 1);
            var test = cache.AttachTestStage();

            cache.InvalidateCache();
            test.AssertInvalidations(1);
        }

        [Test]
        public void MultipleInvalidationsOfCacheDoesNotLeadToMoreThanOneInvalidation()
        {
            var cache = CreateCache(1.AsPipelineConstant());
            var test = cache.AttachTestStage();

            cache.GetValue(); // to validate cache.
            Assert.IsTrue(cache.IsValid);
            test.AssertInvalidations(0);

            cache.InvalidateCache();
            test.AssertInvalidations(1);

            cache.InvalidateCache();
            cache.InvalidateCache();
            cache.InvalidateCache();
            cache.InvalidateCache();
            test.AssertInvalidations(1);
        }

        private class Counter { public int I { get; set; } public void Increment() => I++; }


        public AssignablePipelineStage<int> GetAssignable(int initial) => new AssignablePipelineStage<int>("", initial);
        public CachingPipelineStage<int> CreateCache(IPipelineStage<int> input) => new CachingPipelineStage<int>(input);
    }
}
