using System.Diagnostics;
using System.Threading;

namespace System.Ai {
    public static partial class Fit {
        public const int GENS = (int)1e6;

        public static double binaryLogistic(double[] X, double[] W) {
            Debug.Assert(X.Length == W.Length);
            int len = X.Length;
            double Dot = 0.0;
            for (int j = 0; j < len; j++) {
                Dot += X[j] * W[j];
            }
            var y = Sigmoid.f(Dot);
            return y;
        }

        public static double binaryLogistic(double[] X, double[] W, double label, double lr) {
            Debug.Assert(X.Length == W.Length);
            Debug.Assert(label >= 0 && label <= 1);
            Debug.Assert(lr > 0 && lr <= 1);
            int len = X.Length;
            var y = binaryLogistic(X, W);
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

        public static void train(double lr, Func<double[]> Sample, double[] W, Func<double[], bool> F,
                    Action<double> SetLoss, Func<bool> HasCtrlBreak) {
            if (Sample == null) {
                Console.WriteLine("Sample not found.");
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
                            double loss = 0,
                                cc = 0,
                                err;
                            var X = Sample();
                            bool label = F(X);
                            if (label) {
                                err = -System.Math.Log(binaryLogistic(X, W, 1.0, lr));
                            } else {
                                err = -System.Math.Log(1.0 - binaryLogistic(X, W, 0.0, lr));
                            }
                            if (!double.IsNaN(err) && !double.IsInfinity(err)) {
                                loss += err;
                                cc++;
                            } else {
                                Console.WriteLine("NaN or Infinity detected...");
                            }
                            if (cc > 0) {
                                loss = loss / cc;
                            }
                            SetLoss(loss);
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