using System;
using System.Drawing;
using Microsoft.Win32.Plot2D;

unsafe partial class App {
    static bool ShowMic(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--mic")) {
            cliScript = cliScript.Remove(0, "--mic".Length).Trim();
        } else if (cliScript.StartsWith("fft")) {
            cliScript = cliScript.Remove(0, "mic".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        app.UnMute();
        app.StartWinUI<ISource>(
            onDrawMic, () => app?.Stream, "Fast Fourier Transform (Mic)",
            Color.Black,
            app.onKeyDown);
        return false;
    }

    public static void onDrawMic(Surface2D Canvas, float phase, ISource wav) {
        int hz = wav?.Hz ?? 0;

        var fft = wav?.Peek();

        int samples = wav != null ?
            fft.Length
            : 0;

        double h = samples != 0
            ? hz / (double)samples
            : 0;

        var duration = hz != 0
             ? Math.Round((double)samples / hz, 4)
             : 0;

        Canvas.TopLeft = $"{samples} @ {hz}Hz = {duration}s, {h}Hz";
        Canvas.TopRight = $"{phase:N4}s";

        Canvas.Fill(Color.Black);

        if (fft == null) return;

        var X = Complex.InverseFFT(fft);

        Canvas.Line((x, width) => Color.Green, (x, width) => {
            int i = Surface2D.linear(x, width, X.Length);
            return SigF.f(X[i]) + 0.07;
        });

        Canvas.Line((x, width) => Color.FromArgb(173, 216, 230), (x, width) => {
            int i = Surface2D.linear(x, width, samples / 7);
            return +(SigF.f(2 * fft[i].Magnitude) - 0.5);
        }, true);

        for (int s = 0; s < samples; s++) {
            fft[s].Scale(0.5f);
        }

        X = Complex.InverseFFT(fft);

        Canvas.Line((x, width) => Color.White, (x, width) => {
            int i = Surface2D.linear(x, width, samples / 7);
            return -(SigF.f(2 * fft[i].Magnitude) - 0.5);
        }, true);

        Canvas.Line((x, width) => Color.OrangeRed, (x, width) => {
            int i = Surface2D.linear(x, width, X.Length);
            return SigF.f(X[i]) + 0.07;
        });

        Canvas.Line((x, width) => Color.DarkOrange, (x, width) => {
            int i = Surface2D.linear(x, width, X.Length);
            return SigF.f(X[i]) - 1.07;
        });
    }
}