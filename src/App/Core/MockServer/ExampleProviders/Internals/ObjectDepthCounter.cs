using System;

namespace OpenApiServer.Core.MockServer.ExampleProviders.Internals
{
    public class ObjectDepthCounter
    {
        private int DepthThreshold { get; }
        private int RecursionCounter { get; set; } = 0;
        private int CriticalDepthThreshold { get; }

        public ObjectDepthCounter(int depthThreshold, int criticalDepth = 1000)
        {
            DepthThreshold = depthThreshold;
            CriticalDepthThreshold = criticalDepth;
        }

        public bool HasReachedThreshold => RecursionCounter >= DepthThreshold;
        public bool CanEnter => !HasReachedThreshold;

        public void Increment()
        {
            if (RecursionCounter == CriticalDepthThreshold)
            {
                throw new Exception("Recursion depth has reached its critical level.");
            }
            RecursionCounter++;
        }

        public void Decrement()
        {
            RecursionCounter--;
        }

        public IDisposable Enter()
        {
            Increment();
            return new Decrementer(this);
        }

        private class Decrementer : IDisposable
        {
            private ObjectDepthCounter Counter { get; }

            public Decrementer(ObjectDepthCounter counter)
            {
                Counter = counter;
            }

            public void Dispose()
            {
                Counter.Decrement();
            }
        }
    }
}