namespace System.Audio {
    using System;
    using System.Collections.Generic;
    public static partial class Wav {
        public static IEnumerable<float[]> Synthesize(IEnumerable<Span> music, int Hz, Func<int, int, double> E) {
            foreach (var F in music) {
                if (F.Seconds > 0) {
                    yield return Synthesize(F.Seconds, F, Hz, E);
                }
            }
        }
        public static float[] Synthesize(float seconds, IEnumerable<Frequency> F, int Hz, Func<int, int, double> E) {
            var samples = (int)Math.Ceiling(seconds * (double)Hz);
            float[] signal = new float[samples];
            for (int k = 0; k < signal.Length; k++) {
                double t
                    = 2d * System.Math.PI * k * (1d / (double)Hz);
                signal[k]
                    = Synthesize(F, t)
                        * (float)(E?.Invoke(k, samples) ?? 1d);
            }
            return signal;
        }
        public static float Synthesize(IEnumerable<Frequency> F, double pH) {
            double vol = 0.0d,
                cc = 0.0d;
            foreach (Frequency it in F) {
                if (it.Freq > 0 && it.Vol > 0) {
                    vol += it.Vol /* Vol */
                             * System.Math.Cos(it.Freq /* Freq */ * pH);
                    cc++;
                }
            }
            if (cc > 0) {
                vol /= cc;
            }
            return (float)vol;
        }
    }
}