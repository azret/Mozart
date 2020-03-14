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

    class MicWinUIController : IPlot2DController {
        Mic32 _hMic32;

        public MicWinUIController(Mic32 hMic32) {
            _hMic32 = hMic32;
        }

        public void OnWinShow(IntPtr hWnd, IntPtr wParam, IntPtr lParam) {
            if (hWnd != IntPtr.Zero) {
                UpdateTitle(hWnd);
            }
        }

        private void UpdateTitle(IntPtr hWnd) {
            if (_hMic32.IsMuted) {
                Microsoft.Win32.User32.SetWindowText(hWnd, "Live Recording - OFF");
            } else {
                Microsoft.Win32.User32.SetWindowText(hWnd, "Live Recording - ON");
            }
        }

        public void OnWinMM(IntPtr hWnd, IntPtr wParam, IntPtr lParam) {
            // Mute/UnMute change notification
            if (lParam == IntPtr.Zero && hWnd != IntPtr.Zero
                && wParam == _hMic32.Handle) {
                UpdateTitle(hWnd);
            }
        }
    }

    static bool ShowMicWinUI(App app,
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
        app.StartWinUI<IStream>(new MicWinUIController(app.hMic32),
            DrawPeaks, () => app?.hMic32,
            "Live Recording",
            Color.White,
            app.onKeyDown,
            () => {
                app.Mute();
            },
            Mozart.Properties.Resources.Wave,
            new Size(500, 400));
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
                Notify(hMic, hWaveHeader);
            } else {
                Notify(hMic, hWaveHeader);
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
}