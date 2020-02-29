using System;
using System.Audio;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Win32.Plot2D;
using Microsoft.WinMM;

partial class App {
    static bool OpenMicSignalWindow(App app,
            string cliScript,
            Func<bool> IsTerminated) {
        string title = cliScript;
        if (cliScript.StartsWith("--mic", StringComparison.OrdinalIgnoreCase)) {
            cliScript = cliScript.Remove(0, "--mic".Length).Trim();
        } else if (cliScript.StartsWith("mic", StringComparison.OrdinalIgnoreCase)) {
            cliScript = cliScript.Remove(0, "mic".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        app.Mic?.UnMute();
        app.StartWin32Window<Mic32>(
            onDrawMicWave, () => app.Mic, "Signal (Mic)",
            Color.Black,
            app.onKeyDown);
        return false;
    }

    static bool OpenMicFastFourierTransformWindow(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        string title = cliScript;
        if (cliScript.StartsWith("--fft", StringComparison.OrdinalIgnoreCase)) {
            cliScript = cliScript.Remove(0, "--fft".Length).Trim();
        } else if (cliScript.StartsWith("fft", StringComparison.OrdinalIgnoreCase)) {
            cliScript = cliScript.Remove(0, "fft".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        app.Mic?.UnMute();
        app.StartWin32Window<Mic32>(
            onDrawMicFastFourierTransform, () => app.Mic, "Fast Fourier Transform (Mic)",
            Color.Black,
            app.onKeyDown);
        return false;
    }

    static bool OpenMicMidiWindow(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        string title = cliScript;
        if (cliScript.StartsWith("--md", StringComparison.OrdinalIgnoreCase)) {
            cliScript = cliScript.Remove(0, "--md".Length).Trim();
        } else if (cliScript.StartsWith("md", StringComparison.OrdinalIgnoreCase)) {
            cliScript = cliScript.Remove(0, "md".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        app.Mic?.UnMute();
        app.StartWin32Window<Mic32>(
            onDrawMicFastFourierTransform, () => app.Mic, "MIDI (Mic)",
            Color.Black,
            app.onKeyDown);
        return false;
    }

    static bool PlayFrequency(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        string title = cliScript;

        if (string.IsNullOrWhiteSpace(cliScript)) {
            return false;
        }

        // app.StartWin32Window<Mic32>(
        //     onDrawMicrophoneFastFourierTransform, () => app.Mic, "Fast Fourier Transform (Mic)",
        //     Color.Black,
        //     app.onKeyDown);

        var inFile = @"D:\data\play.md";

        Chord[] inWav = Wav.Parse(inFile);

        var wavOutFile = Path.GetFullPath(Path.ChangeExtension(inFile, ".g.wav"));

        Console.Write($"\r\nSynthesizing...\r\n\r\n");

        var data = Wav.Synthesize(inWav);

        foreach (var it in data) {
            var fft = Complex.ShortTimeFourierTransform(
                it,
                1024,
                Shapes.Square);

            Print.Dump(fft, Wav._hz);

            // Print.Dump(fft);
        }

        Wav.Write(wavOutFile, data);

        Console.Write($"\r\nReady!\r\n\r\n");

        Microsoft.Win32.WinMM.PlaySound(wavOutFile,
            IntPtr.Zero,
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_PURGE);

        return false;
    }

    static void onDrawMicWave(Surface2D Canvas, float phase, Mic32 mic) {
        int m = 10,
                samples = (int)Math.Pow(2, m);

        Canvas.Fill(Canvas._bgColor);

        var data = mic.CH1();

        Debug.Assert(data.Length == samples);

        int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }

        Canvas.Line((x, width) => Color.FromArgb(60, 60, 60), (x, width) => {
            int i = linear(x, width, data.Length);
            return data[i];
        });

        Canvas.Line((x, width) => Color.Orange, (x, width) => {
            int i = linear(x, width, data.Length);
            return SigQ.f(Shapes.Harris(i, data.Length)
                * data[i]) - 0.5;
        });

        double duration
            = Math.Round(samples / (double)mic.Hz, 4);

        Canvas.TopLeft = $"{samples} @ {mic.Hz}Hz = {duration}s";
        Canvas.TopRight = $"{phase:N3}s";
    }
    /*
    static void onDrawMicMidi(Surface2D Canvas, float phase, Mic32 mic) {
        Canvas.Fill(Canvas._bgColor);

        var data = mic.ReadData();

        int samples = mic.Samples;

        double h = Math.Round(mic.Hz
            / (double)samples, 3);

        double Nyquis =
            ((samples / 2)) * h;

        Debug.Assert(data.Length == samples);

        int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }

        float[] re = new float[samples],
            im = new float[samples];

        for (int i = 0; i < samples; i++) {
            var envelope
                = Shapes.Hann(i, samples);
            re[i] = (float)(data[i].CH1 * envelope);
        }

        Complex.FastFourierTransform(
            re,
            im,
            +1);

        float max = 0;

        for (int i = 0; i < samples; i++) {
            re[i] = (float)Math.Sqrt((re[i] * re[i]) + (im[i] * im[i]));
            max = Math.Max(max,
                re[i]);
        }

        im = null;

        if (max > 0) {
            max = 1 / max;
        }

        for (int i = 0; i < samples; i++) {
            re[i] = re[i] * max;
        }

        double[] scale = new double[128],
            cc = new double[128];

        for (int s = 0; s < samples / 2; s++) {
            var f = (s + 1) * h;
            var p = System.Audio.Midi.FreqToKey(f);
            if (p >= 0 && p < scale.Length) {
                scale[p] += re[s];
                cc[p] += 1;
            }
        }

        for (int p = 0; p < scale.Length; p++) {
            if (cc[p] > 0) {
                scale[p] /= cc[p];
            }
        }

        double duration
            = Math.Round(samples / (double)mic.Hz, 4);
        int gg = 0;
        for (int p = 0; p < scale.Length; p++) {
            if (scale[p] > 0) {
                if (gg == 0) {
                    Console.Write($"{duration}s");
                }
                Console.Write($" {System.Audio.Midi.KeyToTone(p)}");
                var dB = Frequency.dB(scale[p]);
                if (dB < 0) {
                    Console.Write($"-{Math.Abs(dB)}dB");
                } else if (dB > 0) {
                    Console.Write($"+{dB}dB");
                } else if (dB == 0) {
                    Console.Write($"±0");
                }
                gg++;
            }
        }
        if (gg > 0) {
            Console.WriteLine();
        }

        Canvas.Line(
            (x, width) => Color.Orange,
            (x, width) => SigQ.f(scale[linear(x, width, scale.Length)]) - 0.5, true
        );

        Canvas.TopLeft = $"{samples} @ {mic.Hz}Hz = {duration}s";
        Canvas.TopRight = $"{phase:N3}s";

        Canvas.BottomLeft = $"{System.Audio.Midi.KeyToFreq(0)}Hz";
        Canvas.BottomRight = $"{System.Audio.Midi.KeyToFreq(scale.Length-1)}Hz";
    }

    static void onDrawMicFastFourierTransformPure(Surface2D Canvas, float phase, Mic32 mic) {
        int samples = mic.Samples;

        double h = Math.Round(mic.Hz
            / (double)samples, 3);

        double Nyquis =
            ((samples / 2)) * h;

        Canvas.Fill(Canvas._bgColor);

        var data = mic.ReadData();

        Debug.Assert(data.Length == samples);

        int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }

        float[] re = new float[samples];
        float[] im = new float[samples];

        for (int i = 0; i < samples; i++) {
            var envelope
                = Shapes.Hann(i, samples);

            re[i] = (float)(data[i].CH1 * envelope);
        }

        Complex.FastFourierTransform(
            re,
            im,
            +1);

        double reNorm = 0;

        for (int i = 0; i < samples; i++) {
            re[i] = (float)Math.Sqrt((re[i] * re[i]) + (im[i] * im[i]));

            reNorm = Math.Max(reNorm,
                re[i]);
        }

        if (reNorm > 0) {
            reNorm = 1 / reNorm;
        }

        for (int i = 0; i < samples; i++) {
            re[i] = (float)(re[i] * reNorm);
        }

        Canvas.Line(
            (x, width) => Color.Orange,
            (x, width) => SigQ.f(re[linear(x, width, samples / 2)]) - 0.5, true
        );
        Canvas.Line(
            (x, width) => Color.Gray,
            (x, width) => -(SigQ.f(re[linear(x, width, samples / 2)]) - 0.5), true
        );

        double duration
            = Math.Round(samples / (double)mic.Hz, 4);

        Canvas.TopLeft = $"{samples} @ {mic.Hz}Hz = {duration}s";
        Canvas.TopRight = $"{phase:N3}s";

        Canvas.BottomLeft = $"{h}Hz";
        Canvas.BottomRight = $"{Nyquis}Hz";
    }
    */

    static void onDrawMicFastFourierTransform(Surface2D Canvas, float phase, Mic32 mic) {
        int samples = mic.Samples;

        double h = Math.Round(mic.Hz
            / (double)samples, 3);

        double Nyquis =
            ((samples / 2)) * h;

        Canvas.Fill(Canvas._bgColor);

        var ch1 = mic.CH1();

        Debug.Assert(ch1.Length == samples);

        int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }

        Func<int, int, double> envelope = null;

        var fft = new Complex[samples];

        for (int s = 0; s < samples; s++) {
            float A = envelope != null
                ? (float)envelope(s, samples)
                : 1.0f;
            fft[s].Re = A *
                ch1[s];
        }

        Complex.FastFourierTransform(
            fft,
            +1);

        // double reNorm = 0;
        // 
        // for (int i = 0; i < samples; i++) {
        //     re[i] = (float)Math.Sqrt((re[i] * re[i]) + (im[i] * im[i]));
        // 
        //     reNorm = Math.Max(reNorm,
        //         re[i]);
        // }
        // 
        // if (reNorm > 0) {
        //     reNorm = 1 / reNorm;
        // }
        // 
        // for (int i = 0; i < samples; i++) {
        //     re[i] = (float)(re[i] * reNorm);
        // }

        Canvas.Line(
            (x, width) => Color.Orange,
            (x, width) => SigQ.f(fft[linear(x, width, samples / 2)].Magnitude) - 0.5, true
        );
        Canvas.Line(
            (x, width) => Color.Gray,
            (x, width) => -(SigQ.f(fft[linear(x, width, samples / 2)].Magnitude) - 0.5), true
        );

        double duration
            = Math.Round(samples / (double)mic.Hz, 4);

        Canvas.TopLeft = $"{samples} @ {mic.Hz}Hz = {duration}s";
        Canvas.TopRight = $"{phase:N3}s";

        Canvas.BottomLeft = $"{h}Hz";
        Canvas.BottomRight = $"{Nyquis}Hz";
    }
}