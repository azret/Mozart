using System;

namespace System.Audio {
    public class Stream : IStream {
        object _lock = new object();

        long _startTime = 0;

        public float GetLocalTime() {
            if (_startTime == 0) { _startTime = Environment.TickCount; }
            return (Environment.TickCount - _startTime) * 0.001f;
        }

        public float Phase {
            get {
                return GetLocalTime();
            }
        }

        public float Hz => 44100;

        float[] _peek;

        public void Push(float[] X) {
            lock (_lock) {
                var last = X != null
                    ? (float[])X.Clone()
                    : null;
                _peek = last;
            }
        }

        public float[] Peek() {
            lock (_lock) {
                var last = _peek != null
                    ? (float[])_peek.Clone()
                    : null;
                return last;
            }
        }
    }
}

