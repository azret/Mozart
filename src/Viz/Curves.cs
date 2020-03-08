using System;
using System.Drawing;
using Microsoft.Win32.Plot2D;

unsafe partial class App {
    static bool ShowFourierTransform(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--fft")) {
            cliScript = cliScript.Remove(0, "--fft".Length).Trim();
        } else if (cliScript.StartsWith("fft")) {
            cliScript = cliScript.Remove(0, "fft".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        var wav = new Stream();
        var X = Sound.Math.Sine(440,
            wav.Hz,
            1024);
        wav.Push(X);
        app.UnMute();
        app.StartWinUI<IStream>(
            DrawFourierTransform, () => wav, "Fast Fourier Transform (Mic)",
            // Color.FromArgb(11, 45, 72),
            Color.Gainsboro,
            null);
        return false;
    }

    public static void DrawFourierTransform(Graphics Canvas, RectangleF fill, float phase, IStream Source) {
        float hz = Source?.Hz ?? 0;

        var X = Source?.Peek();

        int samples = Source != null ?
            X.Length
            : 0;

        double h = samples != 0
            ? hz / (double)samples
            : 0;

        var duration = hz != 0
             ? Math.Round((double)samples / hz, 4)
             : 0;

        if (X == null) return;

        DrawCurve(Color.Black, Canvas, fill, X, 3f);

        Sound.Math.Envelope(X);

        var fft = Complex.FFT(X);

        DrawBars(Color.Black,
            Canvas, fill, fft, (float)System.Math.E, fft.Length / 7);

        Sound.Math.Clean(fft, hz);

        DrawBars(Color.DarkRed,
            Canvas, fill, fft, -(float)System.Math.E, fft.Length / 7);

        X = Complex.InverseFFT(fft);

        DrawCurve(Color.DarkRed, Canvas, fill, X, 2f);
    }

    static void DrawCurve(Color color, Graphics Canvas, RectangleF rect, float[] X, float width = 1f,
        bool bFill = false) {
        var path = new System.Drawing.Drawing2D.GraphicsPath();

        Canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        var Curve = new PointF[(int)rect.Width];

        int M = (int)rect.Height / 2;

        for (int x = 0; x < Curve.Length; x++) {
            var ampl = X[Surface2D.linear(x, Curve.Length, X.Length)];
            if (ampl < -1) ampl = -1;
            if (ampl > 1) ampl = 1;
            if (ampl < -1 || ampl > +1) {
                throw new IndexOutOfRangeException();
            }
            if (!bFill) {
                ampl = (float)SigF.f(ampl) - 0.5f;
            }
            int y = Surface2D.linear(-(float)ampl, 1, M) + M;
            Curve[x] = new PointF(x, y);
        }

        if (bFill) {
            path.AddLines(Curve);
        } else {
            path.AddCurve(Curve);
        }

        var pen = new Pen(color, width);
        var brush = new SolidBrush(color);

        if (bFill)
            Canvas.FillPath(brush, path);
        else
            Canvas.DrawPath(pen, path);

        brush.Dispose();
        pen.Dispose();

        path.Dispose();
    }

    static void DrawBars(Color color, Graphics Canvas, RectangleF rect, Complex[] fft, float norm, int samples) {
        var path = new System.Drawing.Drawing2D.GraphicsPath();

        Canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        int M = (int)rect.Height / 2;

        for (int i = 0; i < samples; i++) {
            var ampl = 2 * fft[i].Magnitude;
            if (ampl < -1) ampl = -1;
            if (ampl > 1) ampl = 1;
            if (ampl < -1 || ampl > +1) {
                throw new IndexOutOfRangeException();
            }
            int x = Surface2D.linear(i, samples, (int)rect.Width);
            int y = Surface2D.linear((float)ampl * norm, 1, M) + M;
            path.AddLine(new PointF(x, y), new PointF(x, y));
        }

        var brush = new SolidBrush(color);
        var pen = new Pen(Color.DarkGray, 1);

        Canvas.FillPath(brush, path);
        Canvas.DrawPath(pen, path);

        pen.Dispose();
        brush.Dispose();

        path.Dispose();
    }
}