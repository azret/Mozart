using System;
using System.Collections.Generic;
using System.Drawing;
using System.Audio;
using Microsoft.Win32.Plot2D;

unsafe partial class Curves {
    public static void DrawCurves(Graphics g, RectangleF r, float t, IStream s) {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        DrawPaper(g, r);
        DrawFunction(g, r, Envelopes.Welch, Brushes.Blue);
        DrawFunction(g, r, Envelopes.Hann, Brushes.Red);
        DrawFunction(g, r, Envelopes.Parabola, Brushes.Green);
        DrawFunction(g, r, Envelopes.Harris, Brushes.Orange);
    }

    private static void DrawPaper(Graphics g,
        RectangleF r,
        byte Xscale = 16,
        byte Yscale = 16) {
        var PixelOffsetMode = g.PixelOffsetMode;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
        var pen = Pens.LightGray;
        for (int x = 0; x < r.Width; x += Xscale) {
            if (x > 0 && x % Xscale == 0) {
                g.DrawLine(pen,
                    new PointF(x, 0),
                    new PointF(x, r.Height));
            }
        }
        for (int y = 0; y < r.Height; y += Yscale) {
            if (y > 0 && y % Yscale == 0) {
                g.DrawLine(pen,
                    new PointF(0, y),
                    new PointF(r.Width, y));
            }
        }
        g.PixelOffsetMode = PixelOffsetMode;
    }

    private static void DrawFunction(Graphics g,
        RectangleF r,
        Func<int, int, double> F,
        Brush brush) {
        float linf(float val, float from, float to) {
            return (val * to / from);
        }
        var PixelOffsetMode = g.PixelOffsetMode;
        if (F != null) {
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            var dots = new List<PointF>();
            int cc = 1024;
            var pen = new Pen(brush, 2f);
            for (int i = 0; i < cc; i++) {
                var ampl = F(i, cc) * 0.997;
                if (ampl < -1 || ampl > +1) {
                    throw new IndexOutOfRangeException();
                }
                float m = r.Height / 2f;
                float y
                    = linf(-(float)ampl, 1f, m) + m;
                float x
                    = linf(i, cc, r.Width);
                dots.Add(new PointF(x, y));
            }
            var pts = dots.ToArray();
            g.DrawCurve(
                pen,
                pts);
            pen.Dispose();
        }
        g.PixelOffsetMode = PixelOffsetMode;
    }

    public static void DrawPeaks(Graphics g,
        RectangleF r,
        float phase,
        IStream Source) {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        DrawPaper(g, r);
        phase = Source?.Phase ?? 0;
        float hz = Source?.Hz ?? 0;
        float[] X =
            Source?.Peek();
        if (X == null) return;
        DrawFunction(g, r, (i, cc) => X[i * X.Length / cc], Brushes.Red);
        string s = $"{phase:n4}s";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.DarkGray, r.Right - 8 - sz.Width,
                 8);
        }
    }

#if x
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

        DrawCurve(Canvas, fill, Color.Black, X, 3f);

        Tools.Envelope(X);

        var fft = Complex.FFT(X);

        DrawBars(Color.Black,
            Canvas, fill, fft, (float)System.Math.E, fft.Length / 7);

        Tools.Clean(fft, hz);

        DrawBars(Color.DarkRed,
            Canvas, fill, fft, -(float)System.Math.E, fft.Length / 7);

        X = Complex.InverseFFT(fft);

        DrawCurve(Canvas, fill, Color.DarkRed, X, 2f);
    }

    private static void DrawDots(Graphics g,
        RectangleF r,
        Func<float, float> F = null,
        byte Xscale = 1,
        byte Yscale = 1) {
        var PixelOffsetMode = g.PixelOffsetMode;
        if (F != null) {
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            var Points = new List<PointF>();
            int cc = (int)r.Width / Xscale;
            for (int i = 0; i < cc + 1; i++) {
                var ampl = F(i / (float)cc);
                if (ampl < -1) ampl = -1;
                if (ampl > 1) ampl = 1;
                if (ampl < -1 || ampl > +1) {
                    throw new IndexOutOfRangeException();
                }
                float y;
                int M = (int)r.Height / 2;
                y = Surface2D.linf(-(float)ampl, 1, M) + M;
                y = ((int)Math.Floor(y / Yscale)) * Yscale;
                var x = (i) * Xscale;
                Points.Add(new PointF(x, y));
            }
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            for (int i = 0; i < Points.Count; i++) {
                var p = Points[i];
                var w = 5;
                if (i > 0) {
                    var p0 = Points[i - 1];
                    path.AddLine(p0, p);
                }
                // path.AddEllipse(
                //     p.X - (w / 2f),
                //     p.Y - (w / 2f),
                //     w,
                //     w);
            }
            // g.FillPath(Pens.DarkRed.Brush, path);
            g.DrawPath(Pens.Gray, path);
            path.Dispose();
            var Curve = new List<PointF>();
            for (int i = 0; i < Points.Count; i++) {
                var p = Points[i];
                if (i > 0) {
                    var p0 = Points[i - 1];
                    if (p0.Y != p.Y) {
                        Curve.Add(p);
                    }
                } else {
                    Curve.Add(p);
                }
            }
            g.DrawCurve(Pens.Orange, Curve.ToArray());
        }

        g.PixelOffsetMode = PixelOffsetMode;
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

        Tools.Envelope(X);

        DrawCurve(g, clientRect, Color.Gray, X, 2f);

        var fft = Complex.FFT(X);

        Tools.Clean(fft, hz);

        X = Complex.InverseFFT(fft);

        DrawCurve(g, clientRect, Color.DarkRed, X, 2f);

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
        Tools.Envelope(X);
        var fft = Complex.FFT(X);
        DrawBars(Color.White,
            g, clientRect, fft, (float)1, fft.Length / 2);
        Tools.Clean(fft, hz);
        DrawBars(Color.DarkRed,
            g, clientRect, fft, -(float)1, fft.Length / 2);
        string s = $"{phase:n4}s";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.LightGray, clientRect.Right - 8 - sz.Width,
                 8);
        }
        DrawCurve(g, clientRect, Color.Green, X, 1f);
    }

    static void DrawCurve(Graphics g, RectangleF clientRect, Color color,
        float[] X, float width = 1f, bool fill = false) {

        var path = new System.Drawing.Drawing2D.GraphicsPath();

        var Curve = new List<PointF>();

        int M = (int)clientRect.Height / 2;

        for (int i = 0; i < X.Length; i++) {
            var ampl = X[i];
            if (ampl < -1) ampl = -1;
            if (ampl > 1) ampl = 1;
            if (ampl < -1 || ampl > +1) {
                throw new IndexOutOfRangeException();
            }
            float x;
            x = Surface2D.linf(i, X.Length, clientRect.Width);
            float y;
            y = Surface2D.linf(-(float)ampl, 1, M) + M;
            Curve.Add(new PointF(x, y));
        }

        path.AddCurve(Curve.ToArray());

        var pen = new Pen(color, width);

        if (fill)
            g.FillPath(pen.Brush, path);
        else
            g.DrawPath(pen, path);

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
#endif
}