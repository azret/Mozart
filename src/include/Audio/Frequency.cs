namespace System.Audio {
    public struct Frequency {
        public static int dB(double amplitude) {
            return (int)(20.0 * System.Math.Log10(amplitude));
        }
        public static double Amplitude(int dB) {
            return System.Math.Pow(10.0, dB / 20.0);
        }
        public float Freq,
            Vol;
        public override string ToString() {
            if (Freq > 0) {
                var vol = dB(Vol);
                if (vol > 0) {
                    return $"{Freq}Hz+{vol}dB";
                } else if (vol < 0) {
                    return $"{Freq}Hz-{System.Math.Abs(vol)}dB";
                } else {
                    return $"{Freq}Hz±0dB";
                };
            } else {
                return $"0Hz";
            }
        }
        // public static Gain[] FromFastFourierTransform(Complex[] fft, int hz) {
        //     int samples = fft.Length;
        //     var F = new Gain[Tones.Length];
        //     var cc = new int[F.Length];
        //     double h = hz
        //         / (double)samples;
        //     for (int s = 0; s < samples / 2; s++) {
        //         var f =
        //                 h * 0.5 + (s * h);
        //         var k = FreqToKey(f);
        //         if (k >= 0 && k < F.Length) {
        //             F[k].Freq = (float)KeyToFreq(k);
        //             F[k].Vol +=
        //                 2 * fft[s].Magnitude;
        //         }
        //     }
        //     for (int k = 0; k < cc.Length; k++) {
        //         if (cc[k] > 0) {
        //             F[k].Vol /= cc[k];
        //         }
        //     }
        //     return F;
        // }
    }
}