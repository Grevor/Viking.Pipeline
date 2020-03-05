using System;

namespace Viking.Pipeline
{
    internal class Suspender
    {
        private int _suspensions;

        public bool IsSuspended => _suspensions > 0;

        public void Suspend() => _suspensions++;
        public bool Resume()
        {
            _suspensions = Math.Max(0, _suspensions - 1);
            return IsSuspended;
        }
    }
}
