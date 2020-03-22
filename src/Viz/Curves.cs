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
            if (x % Xscale == 0) {
                g.DrawLine(pen,
                    new PointF(x, 0),
                    new PointF(x, r.Height));
            }
        }
        for (int y = 0; y < r.Height; y += Yscale) {
            if (y % Yscale == 0) {
                g.DrawLine(pen,
                    new PointF(0, y),
                    new PointF(r.Width, y));
            }
        }
        g.PixelOffsetMode = PixelOffsetMode;
    }

    private static void DrawBars(Graphics g,
        RectangleF r,
        byte Xscale = 16,
        byte Yscale = 16,
        Func<int, float> GetAmplitude = null,
        Brush brush = null) {
        var PixelOffsetMode = g.PixelOffsetMode;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        var pen = Pens.LightGray;
        int i = 0;
        for (int x = 0; x < r.Width; x += Xscale) {
            if (x % Xscale == 0) {
                var ampl = GetAmplitude(i);
                var h = ((int)((int)(r.Height / Yscale) / 2) * ampl) * Yscale;
                var m = (int)((int)(r.Height / Yscale) / 2) * Yscale;
                if (h > 0) {
                    g.FillRectangle(brush,
                        x, m - h, Xscale, h);
                    g.DrawRectangle(pen,
                        x, m - h, Xscale, h);
                    g.FillEllipse(Brushes.Gray, x + (Xscale / 2) - 3,
                        m - h - Yscale / 2 - 3, 7, 7);
                    // g.FillRectangle(Pens.Gray.Brush,
                    //     x + (Xscale / 2) - 1, m - h - 3, 3, h);
                } else if (h < 0) {
                    h *= -1;
                    g.FillRectangle(brush,
                        x, m, Xscale, h);
                    g.DrawRectangle(pen,
                        x, m, Xscale, h);
                    g.FillEllipse(Brushes.Gray, x + (Xscale / 2) - 3,
                        m + h + Yscale / 2 - 3, 7, 7);
                }
                i++;
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

    public static void DrawWave(Graphics g,
        RectangleF r,
        float phase,
        IStream Source) {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        DrawPaper(g, r);
        phase = Source?.ElapsedTime ?? 0;
        float hz = Source?.Hz ?? 0;
        float[] X =
            Source?.Read();
        if (X == null) return;
        DrawFunction(g, r, (i, cc) => Envelopes.Hann(i, cc) * X[i * X.Length / cc], Brushes.Red);
        string s = $"{phase:n4}s";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.DarkGray, r.Right - 8 - sz.Width,
                 8);
        }
    }

    public static void DrawFFT(Graphics g,
        RectangleF r,
        float phase,
        IStream Source) {
        phase = Source?.ElapsedTime ?? 0;
        float hz = Source?.Hz ?? 0;
        float[] X =
            Source?.Read();
        if (X == null) return;
        var fft = Complex.FFT(X);
        int startBin = 0;
        int endBin = 0;
        DrawBars(g, r, 16, 16, (i) => {
            if (i > endBin) {
                endBin = i;
            }
            if (i >= 0 && i < fft.Length) {
                return (2 * fft[i].Magnitude);
            } else {
                return 0f;
            }
        }, Brushes.MediumVioletRed);
        DrawBars(g, r, 16, 16, (i) => {
            if (i > endBin) {
                endBin = i;
            }
            if (i >= 0 && i < fft.Length) {
                return (-2 * fft[i].Magnitude);
            } else {
                return 0f;
            }
        }, Brushes.Violet);

        DrawPaper(g, r);
        //DrawFunction(g, r, (i, cc) => fft[i * (endBin + 1) / cc].Re, Brushes.Red);
        DrawLabels(
            g,
            r,
            phase,
            hz, fft, startBin, endBin);
    }

    static void DrawLabels(Graphics g, RectangleF r, float phase, float hz, Complex[] fft,
        int startBin, int endBin) {
        string s = $"{phase:n2}s";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.DarkGray, r.Right - 8 - sz.Width,
                 8);
        }
        s = $"{fft.Length} at {hz}Hz";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.DarkGray, r.Left + 8,
                 8);
        }
        s = $"{(startBin + 1) * (hz / fft.Length):n2}Hz";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.DarkGray, r.Left + 8,
                  r.Bottom - 8 - sz.Height);
        }
        s = $"{(endBin + 1) * (hz / fft.Length):n2}Hz";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.DarkGray, r.Right - 8 - sz.Width,
                  r.Bottom - 8 - sz.Height);
        }
        s = $"{((endBin + 1) / 2) * (hz / fft.Length):n2}Hz";
        if (s != null) {
            var sz = g.MeasureString(s, Plot2D.Font);
            g.DrawString(
                s, Plot2D.Font, Brushes.DarkGray, r.Left + r.Width / 2 - sz.Width / 2,
                  r.Bottom - 8 - sz.Height);
        }
    }
}