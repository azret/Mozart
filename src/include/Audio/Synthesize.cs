namespace System.Audio {
    using System;
    using System.Collections.Generic;
    public static partial class Wav {
        public static IEnumerable<float[]> Synthesize(IEnumerable<Chord> Frequency) {
            foreach (var it in Frequency) {
                if (it.Seconds > 0) {
                    yield return Synthesize(it);
                }
            }
        }
        public static float[] Synthesize(Chord Frequency) {
            var samples = (int)Math.Ceiling(Frequency.Seconds * _hz);
            float[] signal = new float[samples];
            for (int k = 0; k < signal.Length; k++) {
                double t
                    = 2d * System.Math.PI * k * (1d / _hz);
                signal[k]
                    = Synthesize(Frequency.Frequency, t);
            }
            return signal;
        }
        public static float Synthesize(Frequency[] Frequency, double pH) {
            double vol = 0.0d,
                cc = 0.0d;
            for (int p = 0; p < Frequency.Length; p++) {
                if (Frequency[p].Freq > 0) {
                    vol += Frequency[p].Vol /* Vol */
                             * System.Math.Cos(Frequency[p].Freq /* Freq */ * pH);
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