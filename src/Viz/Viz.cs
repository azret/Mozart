using System;
using System.Audio;
using System.Drawing;
using System.Threading;
using Microsoft.Win32;
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

        app.StartWin32Window<Stereo[]>(
            onDrawFrame, () => elise, title,
            Color.Black);

        return false;
    }

    static void onDrawFrame(Surface2D Canvas, float phase, Stereo[] wav) {
        Canvas.Fill(Color.Black);

        int samples = 1024;

        Complex[] fft = new Complex[samples];

        for (int s = 0; s < samples; s++) {
            var ampl =
                0.7 * Math.Cos(220 * 2d * Math.PI * s * (1d / 44100) + phase);
            ampl +=
                .2 * Math.Cos(440 * 2d * Math.PI * s * (1d / 44100) + phase);
            ampl +=
                0.3 * Math.Cos(880 * 2d * Math.PI * s * (1d / 44100) + phase);
            ampl /= 3;
            fft[s].Re = (float)ampl;
        }

        int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }

        Canvas.Line((x, width) => Color.Orange, (x, width) => {
            int i = linear(x, width, fft.Length);
            return fft[i].Re;
        });

        Complex.FastFourierTransform(fft, +1);

        // for (int b = 0; b < samples / 2; b++) {
        //     fft[b].Re = 0;
        //     fft[b].Im = 0;
        //     int neg = samples - b - 1;
        //     if (neg < samples) {
        //         fft[neg].Re = 0;
        //         fft[neg].Im = 0;
        //     }
        // }

        var z = Midi.FromFastFourierTransform(fft, 44100);

        Complex.FastFourierTransform(fft, -1);

        Canvas.Line((x, width) => Color.Red, (x, width) => {
            int i = linear(x, width, fft.Length);
            return fft[i].Re;
        });

        // Canvas.Line((x, width) => Color.Orange, (x, width) => {
        //     var amp =
        //         Math.Cos(440 * 2d * Math.PI * x * (1d / 44100) + phase);
        //     return amp;
        // });
        // 
        // Canvas.Line((x, width) => Color.Green, (x, width) => {
        //     var amp =
        //         Math.Cos(220 * 2d * Math.PI * x * (1d / 44100));
        //     return amp;
        // });
        // 
        // Canvas.Line((x, width) => Color.Red, (x, width) => {
        //     var amp =
        //         Math.Cos(880 * 2d * Math.PI * x * (1d / 44100));
        //     return amp;
        // });
        // 
        // Canvas.Line((x, width) => Color.White, (x, width) => {
        //     var amp =
        //         Math.Cos(440 * 2d * Math.PI * x * (1d / 44100));
        // 
        //     amp +=
        //         Math.Cos(220 * 2d * Math.PI * x * (1d / 44100));
        // 
        //     amp =
        //         +Math.Cos(880 * 2d * Math.PI * x * (1d / 44100));
        // 
        //     amp /= 3.0;
        // 
        //     return amp;
        // });
        // 
        // Canvas.Line((x, width) => Color.Gray, (x, width) => {
        //     return 0;
        // });
        // 
        // Canvas.Line((x, width) => Color.Gray, (x, width) => {
        //     return 1;
        // });
        // 
        // Canvas.Line((x, width) => Color.Gray, (x, width) => {
        //     return -1;
        // });
        // 
        // Canvas.Line((x, width) => Color.Gray, (x, width) => {
        //     if (wav != null && x < wav.Length) {
        //         return wav[x].Left * 100;
        //     }
        // 
        //     return null;
        // });

        // Canvas.Plot((x, Xaxis) => {
        // 
        //     Pixel2D?[] lines = new Pixel2D?[1];
        // 
        //     lines[0] = new Pixel2D(x.Left,
        //           Surface2D.ChangeColorBrightness(Color.Red,
        //                   (float)(0)));
        // 
        //     return lines;
        // 
        // }, wav, Math.Min(wav.Length, Canvas.Width));

        /*

        double FREQmax = double.MinValue,
            FREQmin = double.MaxValue;

        Canvas.Plot((x, Xaxis) => {
            if (x == null) return null;
            Pixel2D?[] Yaxis = new Pixel2D?[x.Axis.Length + 1];
            for (int j = 0; j < x.Axis.Length; j++) {
                int k = x.Axis.Length - j - 1;
                double vol = x.Axis[k].Re * Math.E;
                if (vol > 0) {
                    FREQmax = Math.Max(FREQmax, x.Axis[k].Im);
                    FREQmin = Math.Min(FREQmin, x.Axis[k].Im);
                }
                if (vol > Math.E) {
                    Color color = Color.White;
                    Yaxis[k] = new Pixel2D(
                          (j / (double)x.Axis.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol >= 1) {
                    Color color = Color.Red;
                    Yaxis[k] = new Pixel2D(
                          (j / (double)x.Axis.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol >= 0.7) {
                    Color color = Color.Yellow;
                    Yaxis[k] = new Pixel2D(
                          (j / (double)x.Axis.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0.5) {
                    Color color = Color.Yellow;
                    Yaxis[k] = new Pixel2D(
                          (j / (double)x.Axis.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0.3) {
                    Color color = Color.YellowGreen;
                    Yaxis[k] = new Pixel2D(
                          (j / (double)x.Axis.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0.1) {
                    Color color = Color.Green;
                    Yaxis[k] = new Pixel2D(
                          (j / (double)x.Axis.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                } else if (vol > 0) {
                    Color color = Color.FromArgb(15, 58, 94);
                    Yaxis[k] = new Pixel2D(
                          (j / (double)x.Axis.Length),
                          Surface2D.ChangeColorBrightness(color,
                                  (float)(0)));
                }
            }

            Yaxis[x.Axis.Length] = new Pixel2D(
                          1 - x.Score.Im,
                          Surface2D.ChangeColorBrightness(Color.Violet,
                                  (float)(0)));

            return Yaxis;

        }, Model.GetBuffer(), cc);

        Canvas.TopLeft = FREQmax.ToString() + "Hz";
        Canvas.BottomLeft = FREQmin.ToString() + "Hz";
        */
    }
}