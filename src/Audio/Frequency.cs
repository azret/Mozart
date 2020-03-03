namespace System.Audio {
    public struct Frequency {
        public readonly float Freq,
            Vol;
        public Frequency(float freq, float vol) {
            Freq = freq;
            Vol = vol;
        }
        public override string ToString() {
            var dB = Audio.dB.FromAmplitude(Vol);
            if (dB > 0) {
                return $"{Freq}Hz+{dB}dB";
            } else if (dB < 0) {
                return $"{Freq}Hz-{System.Math.Abs(dB)}dB";
            } else {
                return $"{Freq}Hz±0dB";
            };
        }
        public static Frequency[] FromFourierTransform(Complex[] fft, int hz) {
            int samples = fft.Length;
            var F = new Frequency[samples / 2];
            double h = hz
                / (double)samples;
            for (int s = 0; s < F.Length; s++) {
                var f =
                        h * 0.5 + (s * h);
                F[s] = new Frequency((float)f, 
                    2 * fft[s].Magnitude);
            }
            return F;
        }
        // public static Frequency[] FromFourierTransformMel(Complex[] fft, int hz) {
        //     int samples = fft.Length;
        //     var F = new Frequency[Midi.Tones.Length];
        //     var cc = new int[F.Length];
        //     double h = hz
        //         / (double)samples;
        //     for (int s = 0; s < samples / 2; s++) {
        //         var f =
        //                 h * 0.5 + (s * h);
        //         var k = Midi.FreqToKey(f);
        //         if (k >= 0 && k < F.Length) {
        //             F[k].Freq = (float)Midi.KeyToFreq(k);
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