using System.Collections.Generic;

namespace System.Audio {
    public static class Process {
        public static IEnumerable<Frequency> Translate(Complex[] fft, int hz) {
            int samples = fft.Length;
            var F = new List<Frequency>();
            double h = hz
                / (double)samples;
            for (int s = 0; s < samples / 2; s++) {
                var f =
                        (s * h);
                var vol = 2 * fft[s].Magnitude;
                if (Ranges.IsInRange(f, dB.FromAmplitude(vol))) {
                    F.Add(new Frequency((float)f,
                        vol));
                }
            }
            return F;
        }
    }
}