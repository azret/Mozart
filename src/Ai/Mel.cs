using System.Audio;
using System.Collections;
using System.Diagnostics;

namespace System.Ai {

    public static partial class Mel {
        public static Vector CreateClassifier(string id) {
            double[] re = new double[CBOW.DIMS],
                im = new double[CBOW.DIMS];
            for (int m = 0; m < CBOW.DIMS; m++) {
                re[m] = ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                im[m] = ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
            }
            Vector it
                = new Vector("𝅘𝅥𝅮", re, im);
            return it;
        }

        public static Matrix CreateModel(int hashSize) {
            var Model = new Matrix(hashSize);
            Model.Push("a");
            foreach (var it in Model) {
                it.Alloc(CBOW.DIMS);
                for (int m = 0; m < CBOW.DIMS; m++) {
                    it.Axis[m].Re = ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                    it.Axis[m].Im = ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                }
            }
            return Model;
        }

        static Vector MelFromFourier(Complex[] STFT) {
            int Fs = STFT.Length * 2;
            Debug.Assert(Fs == 1024 * 2);
            double[] re = new double[CBOW.DIMS],
                    im = new double[CBOW.DIMS],
                cc = new double[CBOW.DIMS];
            for (int i = 0; i < STFT.Length; i++) {
                double h = Wav._hz / (double)Fs,
                     f = i * h;
                var m = Envelopes.FREQ2MIDI(f);
                Debug.Assert(m == Envelopes.FREQ2MIDI(Envelopes.MIDI2FREQ(m)));
                if (m >= 0 && m < CBOW.DIMS) {
                    re[m] += STFT[i].Abs();
                    cc[m] += +1;
                }
            }
            for (int m = 0; m < CBOW.DIMS; m++) {
                // Normalize
                if (cc[m] > 0) {
                    re[m] /= cc[m];
                }
                im[m] = Envelopes.MIDI2FREQ(m);
            }
            Vector it = new Vector("𝆕", re, im);
            var duration = Math.Round((double)Fs / Wav._hz, 5);
            Debug.Assert((int)(duration * Wav._hz) == Fs);
            double scale = 1.6;
            it.Score.Re = scale * duration;
            return it;
        }

        public static Matrix Noise() {
            int Fs = 1024 * 2;
            Debug.Assert(Fs == 1024 * 2);
            var Model = new Matrix(113);
            for (int i = 0; i < Model.Capacity; i++) {
                double[] re = new double[CBOW.DIMS],
                        im = new double[CBOW.DIMS];
                for (int j = 0; j < CBOW.DIMS; j++) {
                    re[j] = ((global::Random.Next() & 0xFFFF) / (65536f))
                        * ((global::Random.Next() & 0xFFFF) / (65536f));
                    im[j] = Envelopes.MIDI2FREQ(j);
                    if (((global::Random.Next() & 0xFFFF) / (65536f)) > 0.3) {
                        re[j] = 0;
                    }
                }
                Vector it = new Vector("𝆕", re, im);
                var duration = Math.Round((double)Fs / Wav._hz, 5);
                Debug.Assert((int)(duration * Wav._hz) == Fs);
                double scale = 1.6;
                it.Score.Re = scale * duration;
                for (int j = 0; j < CBOW.DIMS; j++) {
                    RunSpeachFrequencyFilters(it, j);
                }
                Model[i] = it;
            }
            return Model;
        }

        public static Matrix ShortTimeFourierTransform(Complex[] sig, Set filter, double vol = 0.01, int dbMin = -20, int dbMax = +20) {
            Console.Write($"\r\nBuilding a new Mel model...\r\n");
            double norm = 0.0,
                cc = 0;
            var Model = new Matrix((int)Math.Ceiling(sig.Length / (double)2048));
            foreach (var STFT in Complex.ShortTimeFourierTransform(sig)) {
                int i = Model.Count;
                if (i >= Model.Capacity) {
                    throw new OutOfMemoryException();
                }
                Vector it;
                Debug.Assert(Model[i] == null);
                Model[i] = it = MelFromFourier(STFT);
                for (var j = 0; j < it.Axis.Length; j++) {
                    norm += it.Axis[j].Re;
                    cc++;
                }
            }
            if (cc > 0) {
                norm = norm /= cc;
                if (norm > 0) {
                    norm = 1d / norm;
                }
            }
            for (int i = 0; i < Model.Count; i++) {
                var it = Model[i];
                if (it != null) {
                    for (var j = 0; j < it.Axis.Length; j++) {
                        it.Axis[j].Re = Math.Round(norm
                            /* Note that this surface is really wierd... i.e. There are
                                    positive and negative decibeles */
                            * it.Axis[j].Re / (it.Axis.Length * 0.1), 3);
                        it.Axis[j].Re = it.Axis[j].Re;
                        if (it.Axis[j].Re > 0) {
                            RunSpeachFrequencyFilters(it, j, vol, dbMin, dbMax);
                        }
                        if (it.Axis[j].Re > 0 && filter != null) {
                            /* Filter out the specified pitches */
                            var n = Envelopes.MIDI2NOTE(Envelopes.FREQ2MIDI(it.Axis[j].Im));
                            if (string.IsNullOrWhiteSpace(n) || filter.Has(n)) {
                                it.Axis[j].Re = 0;
                            }
                        }
                        double finalMag = Math.Round(it.Axis[j].Re, 2);
                        it.Axis[j].Re
                            = finalMag;
                    }
                    double dot = 0.0;
                    for (var j = 0; j < it.Axis.Length; j++) {
                        dot += it.Axis[j].Re * 1;
                    }
                    it.Score.Im = Math.Round(
                        Tanh.f(dot), 2);
                }
            }
            return Model;
        }

        static void RunSpeachFrequencyFilters(Vector it, int j, double vol = 0.01, int dbMin = -20, int dbMax = +20) {
            bool IsAudible(double f) {
                if ((f >= 8.1 && f <= 16743.9)) {
                    return true;
                }
                return false;
            }
            bool pass = true;
            if (!IsAudible(it.Axis[j].Im)) {
                pass = false;
            }
            if (it.Axis[j].Re <= 0.01) {
                pass = false;
            }
            if (pass) {
                var n = Envelopes.MIDI2NOTE(Envelopes.FREQ2MIDI(it.Axis[j].Im));
                if (string.IsNullOrWhiteSpace(n)) {
                    pass = false;
                }
                if (n == null || (!n.Contains("3") && !n.Contains("4") && !n.Contains("5")
                                && !n.Contains("6"))) {
                    pass = false;
                }
                if (pass && it.Axis[j].Re > 0) {
                    var dB = Envelopes.dB(it.Axis[j].Re);
                    if (dB <= dbMin || dB >= dbMax) {
                        pass = false;
                    }
                }
            }
            if (!pass) {
                it.Axis[j].Re = 0;
            }
        }
    }
}