using System;
using System.Ai;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;

static partial class App {
    public static Vector[] RunFullCosineSort(IOrthography lex, Matrix Model, string Q, int max) {
        if (Model == null || string.IsNullOrWhiteSpace(Q)) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Model not loaded.\r\n");
            Console.ResetColor();
            Console.WriteLine("See '--load' command for more info...\r\n");
            return null;
        }
        double[] Re = new double[CBOW.DIMS];
        double norm = 0;
        var sign = +1;
        foreach (var tok in PlainText.ForEach(Q, 0, Q.Length, 0)) {
            string wi = lex.GetKey(tok.TextFragment.Substring(tok.StartIndex, tok.Length));
            if (wi == "+") {
                sign = +1;
            } else if (wi == "-") {
                sign = -1;
            } else {
                var vec = Model[wi];
                if (vec != null) {
                    Debug.Assert(vec.Axis.Length == Re.Length);
                    for (var j = 0; j < Re.Length; j++) {
                        Re[j] += sign * vec.Axis[j].Re;
                    }
                    norm++;
                } else {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"'{wi}' not found.");
                    Console.ResetColor();
                }
            }
        }
        if (norm > 0) {
            for (var j = 0; j < Re.Length; j++) {
                Re[j] /= (double)norm;
            }
        }
        Vector[] output = CBOW.Predict(Model, Re, max);
        Array.Sort(output,
            (a, b) => Scalar.CompareTo(a, b));
        Console.WriteLine();
        Console.WriteLine(" [" + string.Join(",", Re.Select(re => Math.Round(re, 4)).Take(7)) + "...]");
        Console.WriteLine();
        int len = 0;
        for (int i = output.Length - 1; i >= 0; i--) {
            Vector n = output[i];
            if (n != null) {
                string str = n.Id;
                var it = Model[n.Id];
                if (it != null) {
                    // if (it.Count > 0) {
                    //     var best = it.ArgMax();
                    //     if (best != null) {
                    //         str = best.Id;
                    //     }
                    // }
                }
                if (len + str.Length > 37  /* break like if does not fit */) {
                    Console.WriteLine(
                        output.Length <= 31
                            ? $" {str} : {n.Score}"
                            : $" {str}");
                    len = 0;
                } else {
                    Console.Write(
                        output.Length <= 31
                            ? $" {str} : {n.Score}"
                            : $" {str}");
                    len += str.Length;
                }
            }
        }
        Console.WriteLine();
        return output;
    }
}