using System;
using System.Audio;

unsafe partial class App {
    long _startTime = 0;
    public float GetLocalTime() {
        if (_startTime == 0) { _startTime = Environment.TickCount; }
        return (Environment.TickCount - _startTime) * 0.001f;
    }

    void Loop() {
        float phase = GetLocalTime();
        phase = 1;

        int samples = 1024;
            int hz = Spectro.Hz;

        var duration =
                Math.Round((double)samples / hz, 4);

        float[] X = new float[samples];

        for (int s = 0; s < samples; s++) {
            var ampl =
                Math.Sin(3 * 43.06640625 * 2d * Math.PI * s * (1d / hz) + phase);
            ampl +=
                Math.Sin(7 * 43.06640625 * 2d * Math.PI * s * (1d / hz) + phase);
            ampl +=
                Math.Sin(11 * 43.06640625 * 2d * Math.PI * s * (1d / hz) + phase);
            ampl /= 3;
            X[s] = (float)ampl;
        }

        // Complex.FastFourierTransform(fft, +1);

        var fft = Complex.FFT(X);

        Print.Dump(fft, Spectro.Hz);

        Spectro.Push(fft);

        Notify(null, IntPtr.Zero);
    }
}