using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Ai {
    public static partial class CBOW {
        public static double PowScale(double score) {
            const double POW = 0.7351;
            const int Xmax = 100;
            if (score <= 1) {
                return 1;
            } else if (score >= Xmax) {
                return System.Math.Pow(Xmax, POW);
            } else {
                return System.Math.Pow(score, POW);
            }
        }

        public const int VERBOSITY = 13;
        public static class Defaults {
            public const double lr = 0.371;
            public const int WINDOW = 13,
                NEGATIVES = 3;
            public const int DIMS = 128,
                THRESHOLD = 7;
        }
        public static double lr = Defaults.lr;
        public static int WINDOW = Defaults.WINDOW,
            NEGATIVES = Defaults.NEGATIVES;
        public static int DIMS = Defaults.DIMS,
            THRESHOLD = Defaults.THRESHOLD;

        public static Vector[] Predict(IEnumerable<Vector> Model, float[] Re, int max) {
            Vector[] best = new Vector[max];
            foreach (Vector c in Model) {
                int b = 0;
                for (int j = 0; j < best.Length; j++) {
                    if (best[j] == null) {
                        b = j;
                        break;
                    }
                    if (best[j].Score.Re < best[b].Score.Re) {
                        b = j;
                    }
                }
                float dot = 0,
                    score;
                for (int j = 0; j < Re.Length; j++) {
                    dot += c.Axis[j].Im * Re[j];
                }
                score = (float)Sigmoid.f(dot);
                if (best[b] == null || best[b].Score.Re < score) {
                    best[b] = new Vector(c.Id, c.HashCode) {
                        Score = new Complex() { Re = score }
                    };
                }
            }
            return best;
        }

        public static string MemSize(double byteCount) {
            string size = "0 Bytes";
            if (byteCount >= 1073741824.0)
                size = String.Format("{0:##.##}", byteCount / 1073741824.0) + " GB";
            else if (byteCount >= 1048576.0)
                size = String.Format("{0:##.##}", byteCount / 1048576.0) + " MB";
            else if (byteCount >= 1024.0)
                size = String.Format("{0:##.##}", byteCount / 1024.0) + " KB";
            else if (byteCount > 0 && byteCount < 1024.0)
                size = byteCount.ToString() + " Bytes";
            return size;
        }

        public static Vector[] CreateNegDistr(Matrix M, int size) {
            Console.Write($"\r\nCreating negative samples...\r\n");
            double norm = 0,
                    cc = 0;
            foreach (Vector g in M) {
                norm += PowScale(g.Score.Re);
                cc++;
            }
            if (norm > 0) {
                norm = 1 / norm;
            }
            Vector[] negDistr = new Vector[0]; int count = 0;
            foreach (Vector g in M) {
                double samples = PowScale(g.Score.Re) * size * norm;
                for (int j = 0; j < samples; j++) {
                    if (count >= negDistr.Length) {
                        Array.Resize(ref negDistr, (int)((negDistr.Length + 7) * 1.75));
                    }
                    negDistr[count++] = g;
                }
            }
            Array.Resize(ref negDistr, count);
            global::Random.Shuffle(
                negDistr,
                negDistr.Length);
            Console.Write($"\r\n  Size: {count}\r\n  Grams: {M.Count}\r\n  Mem: {MemSize(count * Marshal.SizeOf<IntPtr>())}\r\n\r\n");
            return negDistr;
        }

        static Vector PickFromNegDistr(Vector[] negDistr, Vector vec) {
            if (negDistr == null) {
                return null;
            }
            int i = global::Random.Next(negDistr.Length),
                  o = i;
            var neg = negDistr[i];
            while (neg == null || neg.Id.Equals(vec.Id)) {
                i = global::Random.Next(negDistr.Length);
                if (i == o) {
                    return null;
                }
                neg = negDistr[i];
            }
            if (vec.Id.Equals(neg.Id)) {
                return null;
            }
            return neg;
        }

        public static double learnWindow(Matrix Model, Vector[] negDistr, string[] textFragment,
            int iter, Func<bool> HasCtrlBreak, ref int verbOut) {
            Debug.Assert(WINDOW >= 0 && WINDOW <= 13);
            Debug.Assert(NEGATIVES >= 0 && NEGATIVES <= 13);
            Set bow = new Set(),
                    negs = new Set();
            double loss = 0,
                cc = 0;
            Vector wo = null;
            int mid = (textFragment.Length >> 1) % textFragment.Length,
                    stop = mid;
            wo = Model[textFragment[mid]];
            while (wo == null) {
                mid = (mid + 1) % textFragment.Length;
                if (mid == stop) {
                    break;
                }
                wo = Model[textFragment[mid]];
            }
            if (wo == null) {
                return 0;
            }
            if (WINDOW > 0) {
                bow.Clear();
                int boundry = global::Random.Next(WINDOW + 1);
                while (boundry == 0) {
                    boundry = global::Random.Next(WINDOW + 1);
                }
                for (var c = mid - boundry; c <= mid + boundry; c++) {
                    if (c >= 0 && c < textFragment.Length && textFragment[c] != null
                                && !wo.Id.Equals(textFragment[c])) {
                        var wi = Model[textFragment[c]];
                        if (wi != null) {
                            bow.Push(textFragment[c]);
                        }
                    }
                }
                if (bow.Count > 0) {
                    const bool POSITIVE = true;
                    loss += sgd(Matrix.Select(Model, bow, bow.Count),
                        wo,
                        lr,
                        POSITIVE,
                        iter, ref verbOut);
                    cc++;
                }
            }
            if (NEGATIVES > 0) {
                negs.Clear();
                int boundry = global::Random.Next(NEGATIVES + 1);
                while (boundry == 0) {
                    boundry = global::Random.Next(NEGATIVES + 1);
                }
                for (var c = 0; c < boundry; c++) {
                    var neg = PickFromNegDistr(negDistr, wo);
                    if (neg != null
                            && !wo.Id.Equals(neg.Id)
                            && !bow.Has(neg.Id)) {
                        negs.Push(neg.Id);
                    }
                }
                if (negs.Count > 0) {
                    const bool NEGATIVE = false;
                    loss += sgd(Matrix.Select(Model, negs, negs.Count),
                        wo,
                        lr,
                        NEGATIVE,
                        iter, ref verbOut);
                    cc++;
                }
            }
            if (cc > 0) {
                loss /= cc;
            }
            return loss;
        }

        static double sgd(Vector[] Wi, Vector wo,
            double lr, bool label, int gen,
            ref int verbOut) {
            if (Wi == null || Wi.Length <= 0 || wo == null) {
                return 0;
            }
            double loss = 0,
                      cc = 0,
                   score = 0,
                      err = 0;
            double[] input = computeInputVector(Wi);
            double[] grads
                = new double[input.Length];
            score = binaryLogistic(
                input,
                grads,
                wo.Axis,
                label ? 1.0 : 0.0,
                lr);
            if (label) {
                err = -System.Math.Log(score);
            } else {
                err = -System.Math.Log(1.0 - score);
            }
            if (!double.IsNaN(err) && !double.IsInfinity(err)) {
                loss += err;
                cc++;
            } else {
                Console.WriteLine("NaN detected...");
            }
            _LOG_(Wi, wo, label, gen,
                ref verbOut, score, err);
            if (cc > 0) {
                loss = loss / cc;
                for (var j = 0; j < grads.Length; j++) {
                    grads[j] /= (double)cc;
                }
                updateInputVector(Wi, grads);
            }
            return loss;
        }

        static void updateInputVector(Vector[] Wi, double[] grads) {
            foreach (Vector wi in Wi) {
                if (Wi == null) continue;
                Debug.Assert(wi.Axis.Length == grads.Length);
                for (var j = 0; j < wi.Axis.Length; j++) {
                    wi.Axis[j].Re += (float)grads[j];
                }
            }
        }

        static double[] computeInputVector(IEnumerable<Vector> Wi) {
            double[] wi = null;
            int cc = 0;
            foreach (Vector i in Wi) {
                if (i == null) continue;
                if (wi == null) {
                    wi = new double[i.Axis.Length];
                }
                Debug.Assert(wi.Length == i.Axis.Length);
                for (var j = 0; j < wi.Length; j++) {
                    wi[j] += i.Axis[j].Re;
                }
                cc++;
            }
            if (cc > 0) {
                for (var j = 0; j < wi.Length; j++) {
                    wi[j] /= cc;
                }
            }
            return wi;
        }

        public static double binaryLogistic(double[] wi,
            double[] grads, Complex[] wo, double label, double lr) {
            if (wo == null) return 0;
            Debug.Assert(label >= -1 && label <= 1);
            Debug.Assert(lr >= 0 && lr <= 1);
            int len = wi.Length;
            double dot = 0.0;
            for (int j = 0; j < len; j++) {
                dot += wo[j].Im * wi[j];
            }
            var y = Sigmoid.f(dot);
            double diff = lr * (label - y);
            if (double.IsNaN(diff) || double.IsInfinity(diff)) {
                Console.WriteLine("NaN detected...");
                return diff;
            }
            if (grads != null) {
                Debug.Assert(grads.Length == len);
                for (int j = 0; j < len; j++) {
                    grads[j] += wo[j].Im * diff;
                }
            }
            for (int j = 0; j < len; j++) {
                wo[j].Im += (float)(wi[j] * diff);
            }
            return y;
        }

        static void _LOG_(Vector[] input, Vector output, bool label, int gen,
            ref int verbOut, double score, double err) {
            if (err == 0 || (verbOut >= 0 && ((verbOut % VERBOSITY) == 0))) {
                Console.Write($"[{Thread.CurrentThread.ManagedThreadId}.{verbOut}] " +
                    $"p(y = {Convert.ToInt32(label)}, {output.Id} | {string.Join<Vector>(", ", input)})" +
                    $" = {score}\r\n");
            }
            Interlocked.Increment(ref verbOut);
        }
    }
}