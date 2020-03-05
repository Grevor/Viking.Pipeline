using System;
using System.Collections.Generic;

namespace Viking.Pipeline
{
    internal struct AmbivalentPipelineReference : IEquatable<AmbivalentPipelineReference>
    {
        private int HashCode { get; }
        private WeakReference<IPipelineStage>? WeakReference { get; }
        private IPipelineStage? StrongReference { get; }

        public AmbivalentPipelineReference(IPipelineStage target) : this(target, false) { }
        public AmbivalentPipelineReference(IPipelineStage target, bool isStrong)
        {
            if (isStrong)
            {
                StrongReference = target;
                WeakReference = null;
            }
            else
            {
                StrongReference = null;
                WeakReference = new WeakReference<IPipelineStage>(target);
            }

            HashCode = target.GetHashCode();
            GC.KeepAlive(target);
        }

        public bool IsAlive => WeakReference?.TryGetTarget(out var _) ?? true;
        public bool TryGetTarget(out IPipelineStage? target)
        {
            target = StrongReference;
            if (target != null)
                return true;
            return WeakReference!.TryGetTarget(out target);
        }

        public override bool Equals(object obj) => obj is AmbivalentPipelineReference key && Equals(key);
        public bool Equals(AmbivalentPipelineReference other)
        {
            var aAlive = TryGetTarget(out var a);
            var bAlive = other.TryGetTarget(out var b);

            if (aAlive == false && bAlive == false)
                return false;
            return aAlive == bAlive && Equals(a, b);
        }

        public override int GetHashCode() => HashCode;

        public static bool operator ==(AmbivalentPipelineReference left, AmbivalentPipelineReference right) => left.Equals(right);
        public static bool operator !=(AmbivalentPipelineReference left, AmbivalentPipelineReference right) => !(left == right);
    }
}
