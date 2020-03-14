using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Audio;
using Microsoft.Win32;
using Microsoft.Win32.Plot2D;
using Microsoft.WinMM;

unsafe partial class App {
    Mic32 hMic32;

    class MicWinUIController : IPlot2DController {
        Mic32 _hMic32;

        public MicWinUIController(Mic32 hMic32) {
            _hMic32 = hMic32;
        }

        public void WM_CLOSE(IntPtr hWnd, IntPtr wParam, IntPtr lParam) {
            _hMic32?.Mute();
        }

        public void WM_KEYDOWN(IntPtr hWnd, IntPtr wParam, IntPtr lParam) {
            if (wParam == new IntPtr(0x20)) {
                WinMM.PlaySound(null,
                        IntPtr.Zero,
                        WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
                        WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
                        WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
                        WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
                        WinMM.PLAYSOUNDFLAGS.SND_PURGE);
                _hMic32?.Toggle();
            }
        }

        public void WM_SHOWWINDOW(IntPtr hWnd, IntPtr wParam, IntPtr lParam) {
            if (hWnd != IntPtr.Zero) {
                UpdateTitle(hWnd);
            }
        }

        public void WM_WINMM(IntPtr hWnd, IntPtr wParam, IntPtr lParam) {
            if (lParam == IntPtr.Zero && hWnd != IntPtr.Zero
                && wParam == _hMic32.Handle) {
                UpdateTitle(hWnd);
            }
        }

        private void UpdateTitle(IntPtr hWnd) {
            if (_hMic32?.IsMuted == true) {
                Microsoft.Win32.User32.SetWindowText(hWnd, "Live Recording - OFF");
            } else {
                Microsoft.Win32.User32.SetWindowText(hWnd, "Live Recording - ON");
            }
        }
    }

    static bool StartMicWinUI(App app,
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
            Curves.DrawPeaks, () => app?.hMic32,
            "Live Recording",
            Color.White,
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
                if (pwh != null) {
                    (*pwh).dwFlags = (*pwh).dwFlags & ~WaveHeaderFlags.Done;
                }
                WinMM.Throw(
                    WinMM.waveInAddBuffer(hMic.Handle, hWaveHeader, Marshal.SizeOf(typeof(WaveHeader))),
                    WinMM.ErrorSource.WaveIn);
                PostWinUIMessage(hMic, hWaveHeader);
            } else {
                PostWinUIMessage(hMic, hWaveHeader);
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

    public void UnMute() {
        if (hMic32 == null) {
            hMic32 = OpenMic();
        }
        hMic32.UnMute();
    }
}

namespace Microsoft.WinMM {
    public sealed partial class Mic32 : IStream {
        float IStream.Hz => _wfx.nSamplesPerSec;

        public float[] Peek() {
            return CH1();
        }

        public void Push(float[] X) {
            CH1(X);
        }
    }
}