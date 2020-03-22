using System;

namespace System.Audio {
    public interface IStream {
        float ElapsedTime { get; }
        float Hz { get; }
        float[] Read();
        void Write(float[] X);
    }
}