using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct Complex {
        public float Re,
            Im;
        public override string ToString() {
            if (Im > 0) {
                return string.Format("{0}+i{1}", Re, Im);
            } else if (Im < 0) {
                return string.Format("{0}-i{1}", Re, -Im);
            } else {
                return string.Format("{0}±i0", Re);
            }
        }
        public float Magnitude {
            get {
                return (float)Math.Sqrt((Re * Re) + (Im * Im));
            }
        }
    }
    public partial struct Complex {
        public static IEnumerable<Complex[]> ShortTimeFourierTransform(
            float[] data, int samples, Func<int, int, double> envelope) {
            var m = (int)Math.Log(samples, 2);
            if (samples <= 0 || Math.Pow(2, m) != samples) {
                throw new ArgumentException();
            }
            for (int e = 0; e < data.Length; e += samples) {
                Complex[] fft = new Complex[samples];
                if (e + samples <= data.Length) {
                    for (int s = 0; s < samples; s++) {
                        float A = envelope != null
                            ? (float)envelope(s, samples)
                            : 1.0f;
                        fft[s].Re = A *
                            data[s + e];
                    }
                }
                FastFourierTransform(
                    fft,
                    +1);
                yield return fft;
            }
        }
    }
    public partial struct Complex {
        public static void FastFourierTransform(Complex[] fft, short dir) {
            int n = fft.Length,
                    m = (int)Math.Log(n, 2);
            if (Math.Pow(2, m) != n) {
                throw new InvalidOperationException();
            }
            int half = n >> 1,
                    j = 0;
            for (int i = 0; i < n - 1; i++) {
                if (i < j) {
                    float tx = fft[i].Re,
                        ty = fft[i].Im;
                    fft[i].Re = fft[j].Re;
                    fft[i].Im = fft[j].Im;
                    fft[j].Re = tx;
                    fft[j].Im = ty;
                }
                int k = half;
                while (k <= j) {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }
            float c1 = -1.0f,
                c2 = 0.0f;
            int l2 = 1;
            for (int l = 0; l < m; l++) {
                int l1 = l2;
                l2 <<= 1;
                float u1 = 1.0f,
                    u2 = 0.0f;
                for (j = 0; j < l1; j++) {
                    for (int i = j; i < n; i += l2) {
                        int i1 = i + l1;
                        float t1 = u1 * fft[i1].Re - u2 * fft[i1].Im,
                            t2 = u1 * fft[i1].Im + u2 * fft[i1].Re;
                        fft[i1].Re = fft[i].Re - t1;
                        fft[i1].Im = fft[i].Im - t2;
                        fft[i].Re += t1;
                        fft[i].Im += t2;
                    }
                    float z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }
                c2 = (float)Math.Sqrt((1.0 - c1) / 2.0);
                if (dir == 1) {
                    c2 = -c2;
                }
                c1 = (float)Math.Sqrt((1.0 + c1) / 2.0);
            }
            if (dir == 1) {
                for (int i = 0; i < n; i++) {
                    fft[i].Re /= n;
                    fft[i].Im /= n;
                }
            }
        }
    }
}