using System;
using System.Audio;
using System.Threading;

unsafe partial class App {
    long _startTime = 0;
    public float GetLocalTime() {
        if (_startTime == 0) { _startTime = Environment.TickCount; }
        return (Environment.TickCount - _startTime) * 0.001f;
    }

    Timer _hTimer;

    void Loop() {
        float phase = GetLocalTime();

        int samples = 1024;
            int hz = Stream.Hz;

        var duration =
                Math.Round((double)samples / hz, 4);

        float[] X = new float[samples];

        for (int s = 0; s < samples; s++) {
            var f = ((Random.Next() & 0xFFFF));
            var ampl =
                Math.Sin(f * 2d * Math.PI * s * (1d / hz) + phase);
            // ampl +=
            //     Math.Sin(120 * 43.06640625 * 2d * Math.PI * s * (1d / hz) + phase);
            // ampl +=
            //     Math.Sin(240 * Math.PI * s * (1d / hz) + phase);
            // ampl +=
            //     Math.Sin(880 * Math.PI * s * (1d / hz) + phase);
            // ampl +=
            //     Math.Sin(1350 * Math.PI * s * (1d / hz) + phase);
            // ampl +=
            //     Math.Sin(7252 * Math.PI * s * (1d / hz) + phase);
            // ampl /= 7;
            X[s] = (float)ampl;
        }

        var fft = Complex.FFT(X);

        // Print.Dump(fft, Stream.Hz);

        Stream.Push(fft);
    }
}