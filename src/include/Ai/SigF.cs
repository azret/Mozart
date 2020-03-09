using System;
/// <summary>
/// ƒ(a) = 1 / (1 + e⁻ᵃ)
/// </summary>
public class SigF : IFunc {
    public const int MAX = 4;
    public const int _TABLE_SIZE = 512;
    public static double[] _TABLE = new double[_TABLE_SIZE + 1];
    static void initSigmoid() {
        for (int i = 0; i < _TABLE_SIZE + 1; i++) {
            var x = (i * 2 * (double)MAX) / (double)_TABLE_SIZE - (double)MAX;
            _TABLE[i] = 1.0 / (1.0 + Math.Exp(-x));
        }
    }
    static SigF() {
        initSigmoid();
    }
    public static readonly IFunc Ω = New();
    public static IFunc New() {
        return new SigF();
    }
    public override string ToString() {
        return "ƒ(a) = 1 / (1 + e⁻ᵃ)";
    }
    public static double f(double a) {
        if (a < -MAX) {
            return 0.0;
        } else if (a > MAX) {
            return 1.0;
        } else {
            int i = (int)((a + MAX) * _TABLE_SIZE / MAX / 2);
            return _TABLE[i];
        }
    }
    public static double df(double f) {
        return f * (1 - f);
    }
    double IFunc.f(double a) {
        return f(a);
    }
    double IFunc.df(double f) {
        return df(f);
    }
}
