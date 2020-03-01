namespace System.Audio {
    public struct Frequency {
        public float Freq,
            Vol;
        public override string ToString() {
            if (Freq > 0) {
                var dB = Audio.dB.FromAmplitude(Vol);
                if (dB > 0) {
                    return $"{Freq}Hz+{dB}dB";
                } else if (dB < 0) {
                    return $"{Freq}Hz-{System.Math.Abs(dB)}dB";
                } else {
                    return $"{Freq}Hz±0dB";
                };
            } else {
                return $"0Hz";
            }
        }
        public static Frequency[] FromFastFourierTransform(Complex[] fft, int hz) {
            int samples = fft.Length;
            var F = new Frequency[Midi.Tones.Length];
            var cc = new int[F.Length];
            double h = hz
                / (double)samples;
            for (int s = 0; s < samples / 2; s++) {
                var f =
                        h * 0.5 + (s * h);
                var k = Midi.FreqToKey(f);
                if (k >= 0 && k < F.Length) {
                    F[k].Freq = (float)Midi.KeyToFreq(k);
                    F[k].Vol +=
                        2 * fft[s].Magnitude;
                }
            }
            for (int k = 0; k < cc.Length; k++) {
                if (cc[k] > 0) {
                    F[k].Vol /= cc[k];
                }
            }
            return F;
        }
    }
}