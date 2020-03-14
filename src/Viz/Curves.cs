using System;
using System.Collections.Generic;
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
        app.StartWinUI<IStream>(null,
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

    static void DrawPeaks(Graphics g, RectangleF clientRect, float phase, IStream Source) {
        DrawBackPaper(g, clientRect);

        phase = Source?.Phase ?? 0;

        float hz = Source?.Hz ?? 0;

        float[] X =
            Source?.Peek();

        X = Sound.Math.Sine(440, hz, 1024);

        if (X == null) return;

        Sound.Math.Envelope(X);

        DrawCurve(Color.Gray, g, clientRect, X, 2f, false, false);

        var fft = Complex.FFT(X);

        Sound.Math.Clean(fft, hz);

        X = Complex.InverseFFT(fft);

        var peaks = Sound.Math.Peaks(X);

        DrawCurve(Color.DarkRed, g, clientRect, peaks, 2f, false, false);

        string s = $"{phase:n4}s";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.LimeGreen, clientRect.Right - 8 - sz.Width,
                 8);
        }
    }

    private static void DrawBackPaper(Graphics g, RectangleF clientRect) {
        var pen = Pens.LightGray;

        for (int x = 0; x < clientRect.Width; x++) {
            if (x > 0 && x % 13 == 0) {
                g.DrawLine(pen,
                    new PointF(x, 0),
                    new PointF(x, clientRect.Height));
            }
        }
        for (int y = 0; y < clientRect.Height; y++) {
            if (y > 0 && y % 13 == 0) {
                g.DrawLine(pen,
                    new PointF(0, y),
                    new PointF(clientRect.Width, y));
            }
        }
    }

    static void DrawMic(Graphics g, RectangleF clientRect, float phase, IStream Source) {
        phase = Source?.Phase ?? 0;

        float hz = Source?.Hz ?? 0;
        var X = Source?.Peek();
        if (X == null) return;

        var pen = Pens.LightGray;

        for (int x = 0; x < clientRect.Width; x++) {
            if (x > 0 && x % 13 == 0) {
                g.DrawLine(pen,
                    new PointF(x, 0),
                    new PointF(x, clientRect.Height));
            }
        }
        for (int y = 0; y < clientRect.Height; y++) {
            if (y > 0 && y % 13 == 0) {
                g.DrawLine(pen,
                    new PointF(0, y),
                    new PointF(clientRect.Width, y));
            }
        }

        Sound.Math.Envelope(X);

        DrawCurve(Color.Gray, g, clientRect, X, 2f, false, false);

        var fft = Complex.FFT(X);

        Sound.Math.Clean(fft, hz);

        X = Complex.InverseFFT(fft);

        DrawCurve(Color.DarkRed, g, clientRect, X, 2f, false, false);

        string s = $"{phase:n4}s";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.LimeGreen, clientRect.Right - 8 - sz.Width,
                 8);
        }
    }

    static void DrawBars(Graphics g, RectangleF clientRect, float phase, IStream Source) {
        float hz = Source?.Hz ?? 0;
        var X = Source?.Peek();
        if (X == null) return;
        Sound.Math.Envelope(X);
        var fft = Complex.FFT(X);
        DrawBars(Color.White,
            g, clientRect, fft, (float)1, fft.Length / 2);
        Sound.Math.Clean(fft, hz);
        DrawBars(Color.DarkRed,
            g, clientRect, fft, -(float)1, fft.Length / 2);
        string s = $"{phase:n4}s";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.LightGray, clientRect.Right - 8 - sz.Width,
                 8);
        }
        DrawCurve(Color.Green, g, clientRect, X, 1f);
    }

    static void DrawCurve(Color color, Graphics Canvas, RectangleF rect,
        float[] X, float width = 1f, bool fill = false, bool peaksOnly = false) {

        bool[] peaks = new bool[X.Length];
        bool[] troughs = new bool[X.Length];
        for (int i = 0; i < X.Length; i++) {
            if (i > 0 && i < X.Length - 1) {
                if (X[i - 1] < X[i]) {
                    peaks[i - 1] = false;
                    peaks[i] = true;
                } else if (X[i - 1] > X[i]) {
                    troughs[i - 1] = false;
                    troughs[i] = true;
                } else if (X[i - 1] == X[i]) {
                    peaks[i] = true;
                    troughs[i] = true;
                }
            } else {
                peaks[i] = true;
                troughs[i] = true;
            }
        }

        var path = new System.Drawing.Drawing2D.GraphicsPath();

        Canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        var Curve = new List<PointF>();

        int M = (int)rect.Height / 2;

        for (int i = 0; i < X.Length; i++) {
            if (!peaksOnly || (peaksOnly && (troughs[i] || peaks[i]))) {
                var ampl = X[i];
                if (ampl < -1) ampl = -1;
                if (ampl > 1) ampl = 1;
                if (ampl < -1 || ampl > +1) {
                    throw new IndexOutOfRangeException();
                }
                if (!fill) {
                    ampl = (float)SigF.f(ampl) - 0.5f;
                }
                int x = Surface2D.linear(i, X.Length, rect.Width);
                int y = Surface2D.linear(-(float)ampl, 1, M) + M;
                Curve.Add(new PointF(x, y));
            }
        }

        path.AddCurve(Curve.ToArray());

        var pen = new Pen(color, width);

        if (fill)
            Canvas.FillPath(pen.Brush, path);
        else
            Canvas.DrawPath(pen, path);

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