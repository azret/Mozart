using System;
using System.Audio;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Win32.Plot2D;

partial class App {
    static bool ExecViz(App app,
            string cliScript,
            Func<bool> IsTerminated) {
        string title = cliScript;
        if (cliScript.StartsWith("--viz")) {
            cliScript = cliScript.Remove(0, "--viz".Length).Trim();
        } else if (cliScript.StartsWith("viz")) {
            cliScript = cliScript.Remove(0, "viz".Length).Trim();
        } else {
            throw new ArgumentException();
        }

        var elise = Wav.Read(@"D:\data\Elise.wav");

        app.StartWinUI<App>(
            onDrawFrame, () => app, title,
            Color.Black);

        return false;
    }

    static void onDrawFrame(Surface2D Canvas, float phase, App app) {
        Complex[] sdft = app.Spectro.Peek();

        int samples = sdft != null ?
            sdft.Length
            : 0;

        int hz = app.Spectro.Hz;

        var duration =
            Math.Round((double)samples / hz, 4);

        Canvas.TopLeft = $"{samples} @ {hz}Hz = {duration}s";
        Canvas.TopRight = $"{phase:N4}s";

        Canvas.Fill(Color.Black);

        if (sdft == null) return;

        int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }

        var X = Complex.InverseFFT(sdft);

        Canvas.Line((x, width) => Color.Green, (x, width) => {
            int i = linear(x, width, X.Length);
            return SigF.f(X[i]);
        });

        Canvas.Line((x, width) => Color.DarkOrange, (x, width) => {
            int i = linear(x, width, sdft.Length);
            return +(SigF.f(sdft[i].Magnitude) - 0.5);
        }, true);

        for (int s = 0; s < samples; s++) {
            sdft[s].Scale(1f);
        }

        double h = hz
            / (double)samples;

        for (int s = 0; s < samples / 2; s++) {
            var f =
                    h * 0.5 + (s * h);
            var n = samples - s;
            if ((s == 3 || s == 11) && n > s && n >= 0
                        && n < samples) {
                sdft[s].Scale(0f);
                sdft[n].Scale(0f);
            }
        }

        X = Complex.InverseFFT(sdft);

        Canvas.Line((x, width) => Color.Gray, (x, width) => {
            int i = linear(x, width, sdft.Length);
            return -(SigF.f(sdft[i].Magnitude) - 0.5);
        }, true);

        Canvas.Line((x, width) => Color.DarkRed, (x, width) => {
            int i = linear(x, width, X.Length);
            return SigF.f(X[i]);
        });
    }
}