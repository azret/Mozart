using System.Diagnostics;
using System.Threading;

namespace System.Ai {
    public static partial class Fit {
        public const int GENS = (int)1e6;

        public static double binaryLogistic(double[] X, double[] W, double label, double lr) {
            Debug.Assert(label >= -1 && label <= 1);
            Debug.Assert(lr >= 0 && lr <= 1);
            int len = X.Length;
            double Dot = 0.0;
            for (int j = 0; j < len; j++) {
                Dot += X[j] * W[j];
            }
            var y = Tanh.f(Dot);
            double diff = lr * (label - y);
            if (double.IsNaN(diff) || double.IsInfinity(diff)) {
                Console.WriteLine("NaN detected...");
                return diff;
            }
            for (int j = 0; j < len; j++) {
                W[j] += X[j] * diff;
            }
            return y;
        }

        static void Train(double[] X, double[] W, double y,
                    Action<double> SetLoss, Func<bool> HasCtrlBreak) {
            if (X == null) {
                Console.WriteLine("Model not loaded.");
                return;
            }
            Thread[] threads = new Thread[Environment.ProcessorCount * 2];
            int numberOfThreads = 0;
            for (var t = 0; t < threads.Length; t++) {
                threads[t] = new Thread(() => {
                    Interlocked.Increment(ref numberOfThreads);
                    try {
                        for (int iter = 0; iter < GENS; iter++) {
                            if (HasCtrlBreak != null && HasCtrlBreak()) {
                                break;
                            }

                            // if (label) {
                            //     err = -System.Math.Log(score);
                            // } else {
                            //     err = -System.Math.Log(1.0 - score);
                            // }
                            // if (!double.IsNaN(err) && !double.IsInfinity(err)) {
                            //     loss += err;
                            //     cc++;
                            // } else {
                            //     Console.WriteLine("NaN detected...");
                            // }
                            // if (cc > 0) {
                            //     loss = loss / cc;
                            // }

                            SetLoss(0);
                            Thread.Sleep(3000 + global::Random.Next(3000));
                        }
                    } finally {
                        Interlocked.Decrement(ref numberOfThreads);
                    }
                    Console.Write($"[{Thread.CurrentThread.ManagedThreadId}] stopped...\r\n");
                });
            }
            foreach (var t in threads) { t.Start(); }
            foreach (var t in threads) {
                t.Join();
            }
            Debug.Assert(numberOfThreads == 0);
        }
    }
}