using System;
using System.Audio;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Win32.Plot2D;

partial class App {
    static bool ExecViz(App app,
            string cliScript,
            Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--viz")) {
            cliScript = cliScript.Remove(0, "--viz".Length).Trim();
        } else if (cliScript.StartsWith("viz")) {
            cliScript = cliScript.Remove(0, "viz".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        app.StartWinUI<App>(
            onDrawSpectrogram, () => app, "Spectrogram",
            Color.Black,
            app.onKeyDown);
        app.StartWinUI<App>(
            onDrawWave2, () => app, "Wave",
            Color.Black,
            app.onKeyDown);
        return false;
    }

    static void onDrawSpectrogram(Surface2D Canvas, float phase, App app) {
        int hz = app?.Stream?.Hz ?? 0;

        Complex[][] Model = null;

        app?.Stream?.Peek(out Model);

        int cc = Model?.Length ?? 0;

        cc = Math.Max(
            cc,
            Canvas.Width);

        Canvas.Fill(Canvas._bgColor);

        double FREQmax = double.MinValue,
            FREQmin = double.MaxValue;

        Canvas.Plot((it, i) => {

            if (it == null) return null;

            Frequency[] z = Frequency.FromFourierTransform(it, hz);

            Pixel2D?[] Yaxis = new Pixel2D?[z.Length];

            for (int j = 0; j < z.Length; j++) {
                double vol = z[j].Vol * Math.E;
                if (vol > 0) {
                    FREQmax = Math.Max(FREQmax, z[j].Freq);
                    FREQmin = Math.Min(FREQmin, z[j].Freq);
                }
                if (vol > Math.E) {
                    Color color = Color.White;
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol >= 1) {
                    Color color = Color.Red;
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol >= 0.75) {
                    Color color = Color.Yellow;
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0.5) {
                    Color color = Color.YellowGreen;
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0.25) {
                    Color color = Color.Green;
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0.1) {
                    Color color = Color.DarkGreen;
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0.05) {
                    Color color = Color.FromArgb(30, 30, 30);
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0) {
                    Color color = Color.FromArgb(10, 10, 10);
                    Yaxis[j] = new Pixel2D(
                          (j / (double)z.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                }
            }

            return Yaxis;

        }, Model, cc);

        Canvas.BottomLeft = FREQmax.ToString() + "Hz";
        Canvas.TopLeft = FREQmin.ToString() + "Hz";
    }

    static void onDrawWave2(Surface2D Canvas, float phase, App app) {
        Complex[] fft = app?.Stream?.Peek();

        phase = app?.GetLocalTime() ?? 0;

        int samples = fft != null ?
            fft.Length
            : 0;

        int hz = app?.Stream?.Hz ?? 0;

        var duration =
            Math.Round((double)samples / hz, 4);

        Canvas.TopLeft = $"{samples} @ {hz}Hz = {duration}s";
        Canvas.TopRight = $"{phase:N4}s";

        Canvas.Fill(Color.Black);

        if (fft == null) return;

        int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }

        var X = Complex.InverseFFT(fft);

        Canvas.Line((x, width) => Color.Green, (x, width) => {
            int i = linear(x, width, X.Length);
            return SigF.f(X[i]);
        });

        Canvas.Line((x, width) => Color.White, (x, width) => {
            int i = linear(x, width, samples / 3);
            return +(SigF.f(fft[i].Magnitude) - 0.5);
        }, true);

        for (int s = 0; s < samples; s++) {
            fft[s].Scale(1f);
        }

        double h = hz
            / (double)samples;

        Canvas.BottomLeft = $"{h}Hz";

        for (int s = 0; s < samples / 2; s++) {
            var f =
                    h * 0.5 + (s * h);
            var dB = System.Audio.dB.FromAmplitude(2 * fft[s].Magnitude);
            bool filterOut =
                !Ranges.IsInRange(f, dB);
            if (filterOut) {
                var n = samples - s;
                fft[s].Scale(0f);
                if (s > 0 && n > s && n >= 0
                        && n < samples) {
                    fft[n].Scale(0f);
                }
            }
        }

        X = Complex.InverseFFT(fft);

        Canvas.Line((x, width) => Color.Gray, (x, width) => {
            int i = linear(x, width, samples / 3);
            return -(SigF.f(fft[i].Magnitude) - 0.5);
        }, true);

        Canvas.Line((x, width) => Color.OrangeRed, (x, width) => {
            int i = linear(x, width, X.Length);
            return SigF.f(X[i]);
        });

        Canvas.Line((x, width) => Color.DarkOrange, (x, width) => {
            int i = linear(x, width, X.Length);
            return SigF.f(X[i]) - 1.0;
        });
    }
}