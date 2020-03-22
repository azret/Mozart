using System;

namespace System.Audio {
    public class Stream : IStream {
        object _lock = new object();

        long _tickCount = 0;

        public float GetTickCount() {
            if (_tickCount == 0) { _tickCount = Environment.TickCount; }
            return (Environment.TickCount - _tickCount) * 0.001f;
        }

        public float ElapsedTime {
            get {
                return GetTickCount();
            }
        }

        public float Hz => 44100;

        float[] _data;

        public void Write(float[] X) {
            lock (_lock) {
                var last = X != null
                    ? (float[])X.Clone()
                    : null;
                _data = last;
            }
        }

        public float[] Read() {
            lock (_lock) {
                var last = _data != null
                    ? (float[])_data.Clone()
                    : null;
                return last;
            }
        }
    }
}