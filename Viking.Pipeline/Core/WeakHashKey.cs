using System;

namespace Viking.Pipeline
{
    internal struct WeakHashKey<T> : IEquatable<WeakHashKey<T>> where T : class
    {
        private int HashCode { get; }
        private WeakReference<T>? WeakReference { get; }
        private T? StrongReference { get; }

        public WeakHashKey(T target) : this(target, false) { }
        public WeakHashKey(T target, bool isStrong)
        {
            if (isStrong)
            {
                StrongReference = target;
                WeakReference = null;
            }
            else
            {
                StrongReference = null;
                WeakReference = new WeakReference<T>(target);
            }

            HashCode = target.GetHashCode();
            GC.KeepAlive(target);
        }

        public bool IsAlive => WeakReference?.TryGetTarget(out var _) ?? true;
        public bool TryGetTarget(out T? target)
        {
            target = StrongReference;
            if (target != null)
                return true;
            return WeakReference!.TryGetTarget(out target);
        }

        public override bool Equals(object obj) => obj is WeakHashKey<T> key && Equals(key);
        public bool Equals(WeakHashKey<T> other)
        {
            var aAlive = TryGetTarget(out var a);
            var bAlive = TryGetTarget(out var b);

            if (aAlive == false && bAlive == false)
                return HashCode == other.HashCode;
            return aAlive == bAlive && Equals(a, b);
        }

        public override int GetHashCode() => HashCode;

        public static bool operator ==(WeakHashKey<T> left, WeakHashKey<T> right) => left.Equals(right);
        public static bool operator !=(WeakHashKey<T> left, WeakHashKey<T> right) => !(left == right);
    }
}
