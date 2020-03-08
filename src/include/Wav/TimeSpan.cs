using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Audio {
    [DebuggerDisplay("{Seconds}s")]
    public class TimeSpan : IEnumerable<Frequency> {
        public readonly float Seconds;
        readonly
            IEnumerable<Frequency> _keys;
        public TimeSpan(float seconds, IEnumerable<Frequency> keys) {
            Seconds = seconds;
            _keys = keys;
        }
        public IEnumerator<Frequency> GetEnumerator() {
            if (_keys == null) {
                yield break;
            }
            foreach (Frequency f in _keys) {
                if (f.Freq > 0) {
                    yield return f;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return _keys.GetEnumerator();
        }
        public static IEnumerable<float[]> Synthesize(IEnumerable<TimeSpan> M, float Hz, Func<int, int, double> E) {
            foreach (var f in M) {
                if (f.Seconds > 0) {
                    yield return Synthesize(f.Seconds, f, Hz, E);
                }
            }
        }
        public static float[] Synthesize(float seconds, IEnumerable<Frequency> F, float Hz, Func<int, int, double> E) {
            var samples = (int)Math.Ceiling(seconds * (double)Hz);
            float[] X = new float[samples];
            for (int k = 0; k < X.Length; k++) {
                double t
                    = 2d * System.Math.PI * k * (1d / (double)Hz);
                X[k]
                    = Synthesize(F, t)
                        * (float)(E?.Invoke(k, samples) ?? 1d);
            }
            return X;
        }
        public static float Synthesize(IEnumerable<Frequency> F, double t) {
            double vol = 0.0d,
                cc = 0.0d;
            foreach (Frequency it in F) {
                if (it.Freq > 0 && it.Vol > 0) {
                    vol += it.Vol /* Vol */
                             * System.Math.Cos(it.Freq /* Freq */ * t);
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