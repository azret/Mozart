using System.Audio;
using System.Collections;
using System.Diagnostics;

namespace System.Ai {
    public static partial class MIDI {
        static Vector CreateMidiVector(Complex[] fft) {
            int N = fft.Length * 2;
            Debug.Assert(N == 1024);
            double[] im = new double[128];
            Vector vec = new Vector("𝅘𝅥𝅮", null, im);
            var duration = Math.Round(
                (double)N / Wav._hz, 5);
            Debug.Assert((int)(duration * Wav._hz) == N);
            vec.Score.Re = duration;
            vec.Score.Im = N;
            for (int i = 0; i < fft.Length; i++) {
                double h = Wav._hz / (double)N,
                     f = i * h;
                double vol = fft[i].Abs();
                var m = Envelopes.FREQ2MIDI(f);
                if (m >= 0 && m < vec.Axis.Length && vol > 1E-3) {
                    vec.Axis[m].Re += vol;
                    vec.Axis[m].Im += 1;
                }
            }
            for (int m = 0; m < vec.Axis.Length; m++) {
                if (vec.Axis[m].Im == 0) {
                    vec.Axis[m].Re = 0;
                } else {
                    vec.Axis[m].Re /= vec.Axis[m].Im;
                }
                vec.Axis[m].Im = Envelopes.MIDI2FREQ(m);
                Debug.Assert(Envelopes.FREQ2MIDI(vec.Axis[m].Im) == m);
            }
            Debug.Assert(vec.Axis[69].Im == 440);
            return vec;
        }

        public static Matrix ShortTimeFourierTransform(Complex[] waveFragment, int hashSize, string fmt = "MIDI") {
            Console.Write($"\r\nBuilding a new MIDI model...\r\n");
            var Model = new Matrix(hashSize);
            double Xmin = double.MaxValue,
                Xmax = double.MinValue,
                cc = 0;
            foreach (var fft in Complex.ShortTimeFourierTransform(waveFragment)) {
                int t = Model.Count;
                if (t >= Model.Length) {
                    throw new OutOfMemoryException();
                }
                var vec = Model[t] = CreateMidiVector(fft);
                for (var j = 0; j < vec.Axis.Length; j++) {
                    Xmin = Math.Min(Xmin, vec.Axis[j].Re);
                    Xmax = Math.Max(Xmax, vec.Axis[j].Re);
                }
                cc++;
            }
            for (int t = 0; t < Model.Length; t++) {
                if (Model[t] == null) {
                    continue;
                }
                var vec = Model[t];
                for (var j = 0; j < vec.Axis.Length; j++) {
                    vec.Axis[j].Re = (vec.Axis[j].Re - Xmin) /
                                        (Xmax - Xmin);
                    if (vec.Axis[j].Re <= 0.007) {
                        vec.Axis[j].Re = 0;
                    }
                    RunSpeachFrequencyFilters(
                        vec,
                        j);
                }
            }
            var dBmax = int.MinValue;
            for (int t = 0; t < Model.Length; t++) {
                if (Model[t] == null) {
                    continue;
                }
                var vec = Model[t];
                for (var j = 0; j < vec.Axis.Length; j++) {
                    vec.Axis[j].Re = Math.Round(
                        (vec.Axis[j].Re - Xmin) / (Xmax - Xmin), 4);
                    if (vec.Axis[j].Re > 0 && vec.Axis[j].Re < 1E-4) {
                        vec.Axis[j].Re = 0;
                    }
                    RunSpeachFrequencyFilters(
                        vec,
                        j);
                    if (vec.Axis[j].Re > 0) {
                        var dB = Envelopes.AmplitudeTodB(vec.Axis[j].Re);
                        if (dB > dBmax) {
                            dBmax = dB;
                        }
                    }
                }
            }
            if (dBmax == int.MinValue) {
                dBmax = 0;
            }
            for (int t = 0; t < Model.Length; t++) {
                if (Model[t] == null) {
                    continue;
                }
                var vec = Model[t];
                for (var j = 0; j < vec.Axis.Length; j++) {
                    if (vec.Axis[j].Re > 0) {
                        var dB = Envelopes.AmplitudeTodB(vec.Axis[j].Re);
                        vec.Axis[j].Re =
                            Envelopes.dBToAmplitude(dB);
                    }
                }
            }
            return Model;
        }

        static void RunSpeachFrequencyFilters(Vector vec, int j) {
            bool IsAudible(double f) {
                if ((f >= 8.1 && f <= 16743.9)) {
                    return true;
                }
                return false;
            }
            if (!IsAudible(vec.Axis[j].Im)) {
                vec.Axis[j].Re = 0;
            }
            var n = Envelopes.MIDI2NOTE(Envelopes.FREQ2MIDI(vec.Axis[j].Im));
            if (string.IsNullOrWhiteSpace(n) 
                   // || n.Contains("#")
                   // || n.Contains("0")
                   // || n.Contains("1")
                   // || n.Contains("2")
                   // || n.Contains("3")
                   // || n.Contains("4")
                   // || n.Contains("5")
                   // || n == "C6"
                   // || n == "E6"
                   // || n == "D6"
                   // || n == "A6"
                   // || n == "F6"
                   // || n == "G6"
                   // || n == "B6"
                   // || n.Contains("7")
                   // || n.Contains("8")
                   // || n.Contains("9")
                ) {
                vec.Axis[j].Re = 0;
            }
        }
    }
}