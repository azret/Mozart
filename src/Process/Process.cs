using System;
using System.Audio;
using System.Collections.Generic;

public static class Process {
    // for (int s = 0; s < samples / 2; s++) {
    //     // var f = (s * h);
    //     // var dB = System.Audio.dB.FromAmplitude(2 * fft[s].Magnitude);
    //     // bool filterOut =
    //     //     !Ranges.IsInRange(f, dB);
    //     // if (filterOut) {
    //     //     var n = samples - s;
    //     //     fft[s].Scale(0f);
    //     //     if (s > 0 && n > s && n >= 0
    //     //             && n < samples) {
    //     //         fft[n].Scale(0f);
    //     //     }
    //     // }
    // }
    public static float[] Sine(float f, float hz, int samples) {
        float[] X = new float[samples];
        for (int s = 0; s < samples; s++) {
            var ampl =
                Math.Sin(f * 2d * Math.PI * s * (1d / hz));
            ampl +=
                Math.Sin(43 * 7 * 2d * Math.PI * s * (1d / hz));
            ampl /= 2f;
            X[s] = (float)ampl;
        }
        return X;
    }
    public static IEnumerable<Frequency> Translate(Complex[] fft, int hz) {
        int samples = fft.Length;
        var F = new List<Frequency>();
        double h = hz
            / (double)samples;
        for (int s = 0; s < samples / 2; s++) {
            var f =
                    (s * h);
            var vol = 2 * fft[s].Magnitude;
            if (Ranges.IsInRange(f, dB.FromAmplitude(vol))) {
                F.Add(new Frequency((float)f,
                    vol));
            }
        }
        return F;
    }
}
