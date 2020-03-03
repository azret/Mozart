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
        public static double Parse(string t) {
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
    }
}