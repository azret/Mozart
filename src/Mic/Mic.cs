using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.Plot2D;
using Microsoft.WinMM;

namespace Microsoft.WinMM {
    using System;

    public sealed partial class Mic32 : IStream {
        float IStream.Hz => _wfx.nSamplesPerSec;

        public float[] Peek() {
            return CH1();
        }

        public void Push(float[] X) {
            throw new NotImplementedException();
        }
    }
}

unsafe partial class App {
    Mic32 hMic32;

    static bool ShowMic(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--mic")) {
            cliScript = cliScript.Remove(0, "--mic".Length).Trim();
        } else if (cliScript.StartsWith("mic")) {
            cliScript = cliScript.Remove(0, "mic".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        app.UnMute();
        app.StartWinUI<IStream>(
            DrawMic, () => app?.hMic32, "Live Recording - ON",
            Color.White,
            app.onKeyDown,
            () => {
                app.Mute();
            },
            Mozart.Properties.Resources.Wave,
            new Size(300, 200));
        return false;
    }

    Mic32 OpenMic() {
        var hMic32 = new Microsoft.WinMM.Mic32(1024, 44100, (hMic, hWaveHeader) => {
            WaveHeader* pwh = (WaveHeader*)hWaveHeader;
            if (pwh != null) {
                short* psData =
                    (short*)((*pwh).lpData);
                hMic.CaptureData(pwh, psData);
                // var f = 10 * ((float)44100 / (float)1024);
                // f = 440;
                // hMic.CaptureData(Process.Sine(f, hMic.Hz, hMic.Samples));
                var X = hMic.CH1();
                if (pwh != null) {
                    (*pwh).dwFlags = (*pwh).dwFlags & ~WaveHeaderFlags.Done;
                }
                WinMM.Throw(
                    WinMM.waveInAddBuffer(hMic.Handle, hWaveHeader, Marshal.SizeOf(typeof(WaveHeader))),
                    WinMM.ErrorSource.WaveIn);
                //var fft = Complex.FFT(X);
                // Stream.Push(X);
                // Print.Dump(fft, Stream.Hz);
                Notify(null, IntPtr.Zero);
            }
        });
        try {
            hMic32.Open(false);
        } catch (Exception e) {
            Console.Error?.WriteLine(e);
            hMic32.Dispose();
            return null;
        }
        return hMic32;
    }

    public void Toggle() {
        if (hMic32 == null) {
            UnMute();
        } else {
            hMic32?.Toggle();
        }
    }

    public void Mute() {
        hMic32?.Mute();
    }

    public void UnMute() {
        if (hMic32 == null) {
            hMic32 = OpenMic();
        }
        hMic32.UnMute();
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

        DrawCurve(Color.Gray, g, clientRect, X, 2f);

        var fft = Complex.FFT(X);

        Sound.Math.Clean(fft, hz);

        X = Complex.InverseFFT(fft);

        DrawCurve(Color.DarkRed, g, clientRect, X, 2f);

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
}