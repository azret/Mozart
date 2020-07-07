using System;
using System.Audio;
using System.Collections.Generic;

namespace System.Audio {
    public static class Tools {
        public static float[] Sine(float hz, int samples, params float[] f) {
            float[] X = new float[samples];
            for (int s = 0; s < samples; s++) {
                var ampl = 0d;
                for (int j = 0; j < f.Length; j++) {
                    ampl +=
                        System.Math.Sin(f[j] * 2d * System.Math.PI * s * (1d / hz));
                }
                if (f.Length > 0) {
                    ampl /= f.Length;
                }
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
        public static bool[] Peaks(Complex[] X) {
            bool[] peaks = new bool[X.Length];
            bool[] troughs = new bool[X.Length];
            for (int i = 0; i < X.Length; i++) {
                if (i > 0 && i < X.Length - 1) {
                    if (X[i - 1].Magnitude < X[i].Magnitude) {
                        peaks[i - 1] = false;
                        peaks[i] = true;
                    } else if (X[i - 1].Magnitude > X[i].Magnitude) {
                        troughs[i - 1] = false;
                        troughs[i] = true;
                    } else if (X[i - 1].Magnitude == X[i].Magnitude) {
                        peaks[i] = true;
                        troughs[i] = true;
                    }
                } else {
                    peaks[i] = true;
                    troughs[i] = true;
                }
            }
            float[] Y = new float[X.Length];
            for (int i = 0; i < peaks.Length; i++) {
                peaks[i] |= troughs[i];
            }
            return peaks;
        }

        public static bool[] Peaks(float[] X) {
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
            float[] Y = new float[X.Length];
            for (int i = 0; i < peaks.Length; i++) {
                peaks[i] |= troughs[i];
            }
            return peaks;
        }

        public static void CleanInPlace(Complex[] fft, float hz) {
            var samples = fft.Length;
            double h = hz
                / (double)samples;
            for (int s = 0; s < samples / 2; s++) {
                var f = (s * h);
                var vol = 2 * fft[s].Magnitude;
                var dB = System.Audio.dB.FromAmplitude(vol);
                bool filterOut =
                    !Ranges.IsInRange(f, dB);
                var n = samples - s;
                if (filterOut) {
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
