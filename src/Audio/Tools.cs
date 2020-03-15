using System;
using System.Audio;
using System.Collections.Generic;

namespace System.Audio {
    public static class Tools {
        public static float[] Sine(float f, float hz, int samples) {
            float[] X = new float[samples];
            for (int s = 0; s < samples; s++) {
                var ampl =
                    System.Math.Sin(f * 2d * System.Math.PI * s * (1d / hz));
                ampl +=
                    System.Math.Sin(f * System.Math.Sqrt(2) * 2d * System.Math.PI * s * (1d / hz));
                ampl +=
                    System.Math.Sin(f + f * System.Math.Sqrt(2) * 2d * System.Math.PI * s * (1d / hz));
                ampl /= 3f;
                X[s] = (float)ampl;
            }
            return X;
        }
        public static void Envelope(float[] X) {
            var samples = X.Length;
            for (int s = 0; s < samples; s++) {
                X[s] = X[s] 
                    * (float)Envelopes.Hann(s, samples);
            }
        }
        public static float[] Peaks(float[] X) {
            bool[] peaks = new bool[X.Length];
            bool[] troughs = new bool[X.Length];
            for (int i = 0; i < X.Length; i++) {
                if (i > 0 && i < X.Length - 1) {
                    if (X[i - 1] < X[i]) {
                        peaks[i - 1] = false;
                        peaks[i] = true;
                    } else if (X[i - 1] > X[i]) {
                        troughs[i - 1] = false;
                        troughs[i] = true;
                    } else if (X[i - 1] == X[i]) {
                        peaks[i] = true;
                        troughs[i] = true;
                    }
                } else {
                    peaks[i] = true;
                    troughs[i] = true;
                }
            }
            int cc = 0;
            float[] Y = new float[X.Length];
            for (int i = 0; i < X.Length; i++) {
                if (peaks[i] || troughs[i]) {
                    if (cc >= Y.Length) {
                        Array.Resize(ref Y, (int)((Y.Length + 7) * 1.75));
                    }
                    Y[cc++] = X[i];
                }
            }
            Array.Resize(ref Y, cc);
            return Y;
        }
        public static void Clean(Complex[] fft, float hz) {
            var samples = fft.Length;
            double h = hz
                / (double)samples;
            for (int s = 0; s < samples / 2; s++) {
                var f = (s * h);
                var vol = 2 * fft[s].Magnitude;
                var dB = System.Audio.dB.FromAmplitude(vol);
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
        // public static IEnumerable<Frequency> Translate(Complex[] fft, float hz) {
        //     int samples = fft.Length;
        //     var F = new List<Frequency>();
        //     double h = hz
        //         / (double)samples;
        //     double norm = 0;
        //     for (int s = 0; s < samples / 2; s++) {
        //         var f =
        //                 (s * h);
        //         var vol = 2 * fft[s].Magnitude;
        //         if (Ranges.IsInRange(f, dB.FromAmplitude(vol))) {
        //             F.Add(new Frequency((float)f,
        //                 vol));
        //             norm = Math.Max(norm,
        //                 Math.Abs(vol));
        //         }
        //     }
        //     if (norm > 0) {
        //         foreach (var it in F) {
        //             it.Vol /= (float)norm;
        //         }
        //     }
        //     return F;
        // }
    }
}
