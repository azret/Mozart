namespace System.Audio {
    public struct dB {
        public static int FromAmplitude(double amplitude) {
            return (int)(20.0 * System.Math.Log10(amplitude));
        }
        public static double ToAmplitude(int dB) {
            return System.Math.Pow(10.0, dB / 20.0);
        }
    }
}