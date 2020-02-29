namespace System {
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
    }
}