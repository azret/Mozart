using System;
using System.Audio;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Win32.Plot2D;

partial class Viz {
#if de
    static void onDrawSpectrogram(Surface2D Canvas, float phase, App app) {
        int hz = app?.Stream?.Hz ?? 0;

        Complex[][] Model = null;

        // app?.Stream?.Peek(out Model);

        int cc = Model?.Length ?? 0;

        cc = Math.Max(
            cc,
            Canvas.Width);

        Canvas.Fill(Canvas._bgColor);

        double FREQmax = double.MinValue,
            FREQmin = double.MaxValue;

        Canvas.Plot((it, i) => {

            if (it == null) return null;

            // Frequency[] z = Process.Translate(it, hz);
            Frequency[] z = null;

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
#endif
}