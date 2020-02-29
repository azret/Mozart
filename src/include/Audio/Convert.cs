namespace System.Audio {
    using System;
    using System.Diagnostics;
    public static partial class Midi {
        public static string[] Tones = new string[128] {
        "G9",
        "F#9",
        "F9",
        "E9",
        "D#9",
        "D9",
        "C#9",
        "C9",
        "B8",
        "A#8",
        "A8",
        "G#8",
        "G8",
        "F#8",
        "F8",
        "E8",
        "D#8",
        "D8",
        "C#8",
        "C8",
        "B7",
        "A#7",
        "A7",
        "G#7",
        "G7",
        "F#7",
        "F7",
        "E7",
        "D#7",
        "D7",
        "C#7",
        "C7",
        "B6",
        "A#6",
        "A6",
        "G#6",
        "G6",
        "F#6",
        "F6",
        "E6",
        "D#6",
        "D6",
        "C#6",
        "C6",
        "B5",
        "A#5",
        "A5",
        "G#5",
        "G5",
        "F#5",
        "F5",
        "E5",
        "D#5",
        "D5",
        "C#5",
        "C5",
        "B4",
        "A#4",
        "A4",
        "G#4",
        "G4",
        "F#4",
        "F4",
        "E4",
        "D#4",
        "D4",
        "C#4",
        "C4",
        "B3",
        "A#3",
        "A3",
        "G#3",
        "G3",
        "F#3",
        "F3",
        "E3",
        "D#3",
        "D3",
        "C#3",
        "C3",
        "B2",
        "A#2",
        "A2",
        "G#2",
        "G2",
        "F#2",
        "F2",
        "E2",
        "D#2",
        "D2",
        "C#2",
        "C2",
        "B1",
        "A#1",
        "A1",
        "G#1",
        "G1",
        "F#1",
        "F1",
        "E1",
        "D#1",
        "D1",
        "C#1",
        "C1",
        "B0",
        "A#0",
        "A0",
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null
    };
        public static string KeyToTone(int k) {
            if (k >= 0 && k < Tones.Length) {
                return Tones[Tones.Length - k - 1];
            }
            return null;
        }
        public static double ToneToFreq(string t) {
            var freq = 0.0;
            switch (t) {
                case "G9": freq = 12543.85; break;
                case "F#9": freq = 11839.82; break;
                case "F9": freq = 11175.30; break;
                case "E9": freq = 10548.08; break;
                case "D#9": freq = 9956.06; break;
                case "D9": freq = 9397.27; break;
                case "C#9": freq = 8869.84; break;
                case "C9": freq = 8372.02; break;
                case "B8": freq = 7902.13; break;
                case "A#8": freq = 7458.62; break;
                case "A8": freq = 7040.00; break;
                case "G#8": freq = 6644.88; break;
                case "G8": freq = 6271.93; break;
                case "F#8": freq = 5919.91; break;
                case "F8": freq = 5587.65; break;
                case "E8": freq = 5274.04; break;
                case "D#8": freq = 4978.03; break;
                case "D8": freq = 4698.64; break;
                case "C#8": freq = 4434.92; break;
                case "C8": freq = 4186.01; break;
                case "B7": freq = 3951.07; break;
                case "A#7": freq = 3729.31; break;
                case "A7": freq = 3520.00; break;
                case "G#7": freq = 3322.44; break;
                case "G7": freq = 3135.96; break;
                case "F#7": freq = 2959.96; break;
                case "F7": freq = 2793.83; break;
                case "E7": freq = 2637.02; break;
                case "D#7": freq = 2489.02; break;
                case "D7": freq = 2349.32; break;
                case "C#7": freq = 2217.46; break;
                case "C7": freq = 2093.00; break;
                case "B6": freq = 1975.53; break;
                case "A#6": freq = 1864.66; break;
                case "A6": freq = 1760.00; break;
                case "G#6": freq = 1661.22; break;
                case "G6": freq = 1567.98; break;
                case "F#6": freq = 1479.98; break;
                case "F6": freq = 1396.91; break;
                case "E6": freq = 1318.51; break;
                case "D#6": freq = 1244.51; break;
                case "D6": freq = 1174.66; break;
                case "C#6": freq = 1108.73; break;
                case "C6": freq = 1046.50; break;
                case "B5": freq = 987.77; break;
                case "A#5": freq = 932.33; break;
                case "A5": freq = 880.00; break;
                case "G#5": freq = 830.61; break;
                case "G5": freq = 783.99; break;
                case "F#5": freq = 739.99; break;
                case "F5": freq = 698.46; break;
                case "E5": freq = 659.26; break;
                case "D#5": freq = 622.25; break;
                case "D5": freq = 587.33; break;
                case "C#5": freq = 554.37; break;
                case "C5": freq = 523.25; break;
                case "B4": freq = 493.88; break;
                case "A#4": freq = 466.16; break;
                case "A4": freq = 440.00; break;
                case "G#4": freq = 415.30; break;
                case "G4": freq = 392.00; break;
                case "F#4": freq = 369.99; break;
                case "F4": freq = 349.23; break;
                case "E4": freq = 329.63; break;
                case "D#4": freq = 311.13; break;
                case "D4": freq = 293.66; break;
                case "C#4": freq = 277.18; break;
                case "C4": freq = 261.63; break;
                case "B3": freq = 246.94; break;
                case "A#3": freq = 233.08; break;
                case "A3": freq = 220.00; break;
                case "G#3": freq = 207.65; break;
                case "G3": freq = 196.00; break;
                case "F#3": freq = 185.00; break;
                case "F3": freq = 174.61; break;
                case "E3": freq = 164.81; break;
                case "D#3": freq = 155.56; break;
                case "D3": freq = 146.83; break;
                case "C#3": freq = 138.59; break;
                case "C3": freq = 130.81; break;
                case "B2": freq = 123.47; break;
                case "A#2": freq = 116.54; break;
                case "A2": freq = 110.00; break;
                case "G#2": freq = 103.83; break;
                case "G2": freq = 98.00; break;
                case "F#2": freq = 92.50; break;
                case "F2": freq = 87.31; break;
                case "E2": freq = 82.41; break;
                case "D#2": freq = 77.78; break;
                case "D2": freq = 73.42; break;
                case "C#2": freq = 69.30; break;
                case "C2": freq = 65.41; break;
                case "B1": freq = 61.74; break;
                case "A#1": freq = 58.27; break;
                case "A1": freq = 55.00; break;
                case "G#1": freq = 51.91; break;
                case "G1": freq = 49.00; break;
                case "F#1": freq = 46.25; break;
                case "F1": freq = 43.65; break;
                case "E1": freq = 41.20; break;
                case "D#1": freq = 38.89; break;
                case "D1": freq = 36.71; break;
                case "C#1": freq = 34.65; break;
                case "C1": freq = 32.70; break;
                case "B0": freq = 30.87; break;
                case "A#0": freq = 29.14; break;
                case "A0": freq = 27.50; break;
                default: freq = double.Parse(t); break;
            }
            return freq;
        }
        public static int FreqToKey(double f) {
            var k = (int)Math.Round(12 *
                Math.Log(Math.Round(f, 2) / 440d, 2) + 69);
            return k;
        }
        public static double KeyToFreq(int k) {
            var f = Math.Round(
                Math.Pow(2d, (k - 69) / 12d) * 440, 2);
            Debug.Assert(FreqToKey(f) == k);
            return f;
        }
        public static Frequency[] FromFastFourierTransform(Complex[] fft, int hz) {
            int samples = fft.Length;
            var F = new Frequency[Tones.Length];
            var cc = new int[F.Length];
            double h = hz
                / (double)samples;
            for (int s = 0; s < samples / 2; s++) {
                var f =
                    h * 0.5 + (s * h);
                var k = FreqToKey(f);
                if (k >= 0 && k < F.Length) {
                    F[k].Freq = (float)KeyToFreq(k);
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