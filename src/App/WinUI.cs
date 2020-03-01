using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.Plot2D;

unsafe partial class App {
    Thread StartWinUI<T>(Plot2D<T>.DrawFrame onDrawFrame, Func<T> onGetFrame, string title,
        Color bgColor, Plot2D<T>.KeyDown onKeyDown = null)
        where T : class {
        Thread t = new Thread((getFrame) => {
            IntPtr handl = IntPtr.Zero;
            Plot2D<T> hWnd = null;
            try {
                hWnd = new Plot2D<T>($"{title}",
                    onDrawFrame, onKeyDown,
                    TimeSpan.FromMilliseconds(1000),
                    onGetFrame, bgColor);
                hWnd.Show();
                AddHandle(handl = hWnd.hWnd);
                while (User32.GetMessage(out MSG msg, hWnd.hWnd, 0, 0) != 0) {
                    User32.TranslateMessage(ref msg);
                    User32.DispatchMessage(ref msg);
                }
            } catch (Exception e) {
                Console.Error?.WriteLine(e);
            } finally {
                RemoveHandle(handl);
                hWnd?.Dispose();
                WinMM.PlaySound(null,
                        IntPtr.Zero,
                        WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
                        WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
                        WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
                        WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
                        WinMM.PLAYSOUNDFLAGS.SND_PURGE);
            }
        });
        t.Start(onGetFrame);
        return t;
    }

    int onKeyDown<T>(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam, T frame) {
        WinMM.PlaySound(null,
                IntPtr.Zero,
                WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
                WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
                WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
                WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
                WinMM.PLAYSOUNDFLAGS.SND_PURGE);
        Mic?.Toggle();
        return 0;
    }

    #region Events

    object _lock = new object();

    private IntPtr[] _handles;

    public void AddHandle(IntPtr hWnd) {
        lock (_lock) {
            if (_handles == null) {
                _handles = new IntPtr[0];
            }
            Array.Resize(ref _handles,
                _handles.Length + 1);
            _handles[_handles.Length - 1] = hWnd;
        }
    }

    public void ClearHandles() {
        lock (_lock) {
            _handles = null;
        }
    }

    public void RemoveHandle(IntPtr hWnd) {
        lock (_lock) {
            if (_handles != null) {
                for (int i = 0; i < _handles.Length; i++) {
                    if (_handles[i] == hWnd) {
                        _handles[i] = IntPtr.Zero;
                    }
                }
            }
        }
    }

    public unsafe void Notify(Microsoft.WinMM.Mic32 hMic, IntPtr hWaveHeader) {
        lock (_lock) {
            if (_handles == null) {
                return;
            }
            foreach (IntPtr hWnd in _handles) {
                if (hWnd != IntPtr.Zero) {
                    User32.PostMessage(hWnd, WM.WINMM,
                        hMic != null
                            ? hMic.Handle
                            : IntPtr.Zero,
                        hWaveHeader);
                }
            }
        }
    }

    #endregion
}