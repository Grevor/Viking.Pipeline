using System;
using System.Collections.Generic;

namespace Viking.Pipeline
{
    internal struct AmbivalentReference<T> : IEquatable<AmbivalentReference<T>> where T : class
    {
        public static IEqualityComparer<AmbivalentReference<T>> InequalityOnDeadComparer { get; } = new InequalityOnDead();

        private int HashCode { get; }
        private WeakReference<T>? WeakReference { get; }
        private T? StrongReference { get; }

        public AmbivalentReference(T target) : this(target, false) { }
        public AmbivalentReference(T target, bool isStrong)
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

        public override bool Equals(object obj) => obj is AmbivalentReference<T> key && Equals(key);
        public bool Equals(AmbivalentReference<T> other)
        {
            var aAlive = TryGetTarget(out var a);
            var bAlive = other.TryGetTarget(out var b);

            if (aAlive == false && bAlive == false)
                return HashCode == other.HashCode;
            return aAlive == bAlive && Equals(a, b);
        }

        public override int GetHashCode() => HashCode;

        public static bool operator ==(AmbivalentReference<T> left, AmbivalentReference<T> right) => left.Equals(right);
        public static bool operator !=(AmbivalentReference<T> left, AmbivalentReference<T> right) => !(left == right);

        private class InequalityOnDead : IEqualityComparer<AmbivalentReference<T>>
        {
            public bool Equals(AmbivalentReference<T> x, AmbivalentReference<T> y)
            {
                var aAlive = x.TryGetTarget(out var a);
                var bAlive = y.TryGetTarget(out var b);

                if (aAlive == false && bAlive == false)
                    return false;
                return aAlive == bAlive && Equals(a, b);
            }

            public int GetHashCode(AmbivalentReference<T> obj)
            {
                return obj.HashCode;
            }
        }
    }
}
