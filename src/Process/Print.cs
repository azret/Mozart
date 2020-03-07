using System;
using System.Audio;
using System.Collections.Generic;
using System.IO;

public static class Print {
    public static void Dump(IEnumerable<Complex[]> fft) {
        foreach (Complex[] i in fft) {
            int cc = 0;
            for (int s = 0; s < i.Length; s++) {
                if (s == 0) {
                    Console.Write($"{i.Length}");
                }
                Console.Write($" {i[s].ToString()}");
                cc++;
            }
            if (cc > 0) {
                Console.WriteLine();
            }
        }
    }

    public static void Dump(TextWriter Console, IEnumerable<Frequency[]> span, int samples, int hz) {
        foreach (Frequency[] i in span) {
            Dump(Console, i, samples, hz);
        }
    }

    public static void Dump(TextWriter Console, IEnumerable<Frequency> span, int samples, int hz) {
        double duration
            = Math.Round(samples / (double)hz, 4);
        double h = hz
            / (double)samples;
        int cc = 0;
        foreach (var it in span) {
            var dB = System.Audio.dB.FromAmplitude(it.Vol);
            if ((dB != int.MinValue) && it.Vol > 0 && it.Vol <= 1) {
                if (cc == 0) {
                    Console.Write($"{duration}s ║");
                }
                if (dB < 0) {
                    Console.Write($" {it.Freq:n2}Hz{dB}dB");
                } else if (dB > 0) {
                    Console.Write($" {it.Freq:n2}Hz+{dB}dB");
                } else {
                    Console.Write($" {it.Freq:n2}Hz±0dB");
                }
                cc++;
            }
        }
        if (cc > 0) {
            Console.WriteLine();
        }
    }
}
