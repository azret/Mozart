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
    int step = 0;
    void Loop() {
        float phase = GetLocalTime();

        int samples = 2048;
            int hz = Stream.Hz;

        var duration =
                Math.Round((double)samples / hz, 4);

        float[] X = new float[samples];

        step++;

        for (int s = 0; s < samples; s++) {
            var f = Random.Next() % hz;
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

        if (step >= 450) {
            for (int s = 0; s < samples; s++) {
                fft[s].Scale(1f);
            }

            double h = hz
                / (double)samples;

            for (int s = 0; s < samples / 2; s++) {
                var f =
                        h * 0.5 + (s * h);
                var dB = System.Audio.dB.FromAmplitude(2 * fft[s].Magnitude);
                bool filterOut =
                    !Ranges.IsInRange(f, dB);
                if (filterOut) {
                    var n = samples - s;
                    fft[s].Scale(0f);
                    if (s > 0 && n > s && n >= 0
                            && n < samples) {
                        fft[n].Scale(0f);
                    }
                }
            }
        }

        Stream.Push(fft);
    }
}