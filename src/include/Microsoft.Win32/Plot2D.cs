﻿namespace Microsoft.Win32.Plot2D {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32;

    public class Plot2D<T> : IDisposable
        where T: class {
        public delegate void DrawFrame(Surface2D g, float t, T userState);
        public enum SystemIcons {
            IDI_APPLICATION = 32512,
            IDI_HAND = 32513,
            IDI_QUESTION = 32514,
            IDI_EXCLAMATION = 32515,
            IDI_ASTERISK = 32516,
            IDI_WINLOGO = 32517,
            IDI_WARNING = IDI_EXCLAMATION,
            IDI_ERROR = IDI_HAND,
            IDI_INFORMATION = IDI_ASTERISK,
        }
        Surface2D hSurface2D;
        public IntPtr hWnd;
        Timer hTimer;
        public readonly Font Font = new Font("Consolas", 13f);
        long _startTime = 0;
        public float GetLocalTime() {
            if (_startTime == 0) { _startTime = Environment.TickCount; }
            return (Environment.TickCount - _startTime) * 0.001f;
        }
        public readonly Func<T> _getFrame;
        DrawFrame _onDrawFrame;
        KeyDown _onKeyDown;
        int DefWndProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam) {
            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }
        int UserWndProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam) {
            if (hWnd != this.hWnd) {
                return User32.DefWindowProc(hWnd, (WM)msg, wParam, lParam);
            }
            switch ((WM)msg) {
                case WM.WINMM:
                    OnWinMM(hWnd, wParam, lParam);
                    break;
                case WM.KEYDOWN:
                    if (_onKeyDown != null) {
                        return _onKeyDown(hWnd, msg, wParam, lParam, _getFrame != null ?
                            _getFrame() : null);
                    }
                    return 0;
                case WM.SIZE:
                case WM.SIZING:
                    User32.GetClientRect(hWnd, out RECT lprctw);
                    User32.InvalidateRect(hWnd, ref lprctw, false);
                    break;
                case WM.PAINT:
                    OnPaint(_onDrawFrame, _getFrame, 1, hWnd);
                    return 0;
                case WM.DESTROY:
                    Dispose();
                    User32.PostQuitMessage(0);
                    return 0;
            }
            return User32.DefWindowProc(hWnd, (WM)msg, wParam, lParam);
        }
        Color _bgColor;
        WndProc lpfnWndProcPtr;
        readonly GCHandle lpfnWndProcGCHandle;
        public class ClassTemplate {
            internal Icon hIcon;
            internal string szName = "WNDCLASSEX_PLOT2D";
            internal WNDCLASSEX _lpwcx;
            internal WndProc lpfnDefWndProcPtr = new WndProc(User32.DefWindowProc);
        }
        ClassTemplate _ClassTemplate;
        public Plot2D(string title, DrawFrame onDrawFrame, KeyDown onKeyDown, TimeSpan framesPerSecond,
            Func<T> getFrame, Color bgColor) {
            _getFrame = getFrame;
            _onDrawFrame = onDrawFrame;
            _onKeyDown = onKeyDown;
            _bgColor = bgColor;
            if (_ClassTemplate == null) {
                _ClassTemplate = new ClassTemplate();
                _ClassTemplate._lpwcx = new WNDCLASSEX();
                _ClassTemplate._lpwcx.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
                _ClassTemplate._lpwcx.hInstance = User32.GetModuleHandle(null);
                _ClassTemplate.hIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
                _ClassTemplate._lpwcx.style = (int)(ClassStyles.HorizontalRedraw | ClassStyles.VerticalRedraw);
                _ClassTemplate._lpwcx.cbClsExtra = 0;
                _ClassTemplate._lpwcx.cbWndExtra = 0;
                _ClassTemplate._lpwcx.hCursor = User32.LoadCursor(IntPtr.Zero, (int)Constants.IDC_ARROW);
                _ClassTemplate._lpwcx.hbrBackground = User32.CreateSolidBrush(ColorTranslator.ToWin32(_bgColor));
                _ClassTemplate._lpwcx.lpszMenuName = null;
                _ClassTemplate._lpwcx.lpszClassName = _ClassTemplate.szName;
                _ClassTemplate._lpwcx.hIcon = _ClassTemplate.hIcon.Handle;
                _ClassTemplate._lpwcx.lpfnWndProc = _ClassTemplate.lpfnDefWndProcPtr;
                if (User32.RegisterClassEx(ref _ClassTemplate._lpwcx) == 0) {
                    if (1410 != Marshal.GetLastWin32Error()) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }
            hWnd = User32.CreateWindowEx(
                    WindowStylesEx.WS_EX_APPWINDOW,
                    _ClassTemplate.szName,
                    title,
                    WindowStyles.WS_OVERLAPPED |
                    WindowStyles.WS_SYSMENU |
                    WindowStyles.WS_BORDER |
                    WindowStyles.WS_SIZEFRAME |
                    WindowStyles.WS_MINIMIZEBOX |
                    WindowStyles.WS_MAXIMIZEBOX,
                160,
                230,
                720,
                (int)(720 * (3f / 5f)),
                IntPtr.Zero,
                IntPtr.Zero,
                _ClassTemplate._lpwcx.hInstance,
                IntPtr.Zero);
            if (hWnd == IntPtr.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            lpfnWndProcPtr = new WndProc(UserWndProc);
            lpfnWndProcGCHandle = GCHandle.Alloc(lpfnWndProcPtr, GCHandleType.Normal);
            User32.SetWindowLongPtr(hWnd,
                (int)User32.WindowLongFlags.GWL_WNDPROC,
                lpfnWndProcPtr);
            hTimer = new Timer((state) => {
                User32.GetClientRect(hWnd, out RECT lprctw);
                User32.InvalidateRect(hWnd, ref lprctw, false);
            }, null, 0, (int)framesPerSecond.TotalMilliseconds);
        }
        public void Dispose() {
            hTimer?.Dispose();
            hTimer = null;
            if (hWnd != IntPtr.Zero) {
                User32.ShowWindow(hWnd, ShowWindowCommands.Hide);
                User32.DestroyWindow(hWnd);
            }
            hWnd = IntPtr.Zero;
            lpfnWndProcGCHandle.Free();
            lpfnWndProcPtr = null;
            hSurface2D?.Dispose();
        }

        public delegate int KeyDown(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam, T GetSession);
        void OnPaint(DrawFrame onDrawFrame, Func<T> userState, float scale, IntPtr hWnd) {
            IntPtr hdc = User32.BeginPaint(hWnd, out PAINTSTRUCT ps);
            User32.GetClientRect(hWnd,
                out RECT lprct);
            RECT Membitrect = new RECT(0, 0, (int)((lprct.Right - lprct.Left) * scale),
                (int)((lprct.Bottom - lprct.Top) * scale));
            if (hSurface2D == null || hSurface2D.Width != new Surface2D.Quantum(Membitrect.Right - Membitrect.Left).Value
                 || hSurface2D.Height != new Surface2D.Quantum(Membitrect.Bottom - Membitrect.Top).Value) {
                Interlocked.Exchange(ref hSurface2D, new Surface2D(new Surface2D.Quantum(Membitrect.Right - Membitrect.Left),
                    new Surface2D.Quantum(Membitrect.Bottom - Membitrect.Top), _bgColor))?.Dispose();
            }
            IntPtr Memhdc = User32.CreateCompatibleDC(hdc);
            IntPtr Membitmap = User32.CreateCompatibleBitmap(hdc,
                Membitrect.Right - Membitrect.Left,
                Membitrect.Bottom - Membitrect.Top);
            User32.SelectObject(Memhdc, Membitmap);
            if (_ClassTemplate._lpwcx.hbrBackground != IntPtr.Zero) {
                User32.FillRect(Memhdc, ref Membitrect,
                        _ClassTemplate._lpwcx.hbrBackground);
            }
            Graphics _g = Graphics.FromHdc(Memhdc);
            hSurface2D.BeginPaint();
            var phase = GetLocalTime();
            try {
                onDrawFrame?.Invoke(hSurface2D,
                    phase, userState != null ?
                        userState.Invoke() : null);
            } catch {
            }
            hSurface2D.EndPaint();

            if (hSurface2D.hBitMap != null) {
                _g.DrawImage(hSurface2D.hBitMap,
                    0, 0);
            }
            if (hSurface2D.TopLeft != null) {
                _g.DrawString(
                    $"{hSurface2D.TopLeft}", Font, Brushes.LimeGreen, 6, 8);
            }
            if (hSurface2D.TopRight != null) {
                var sz = _g.MeasureString($"{hSurface2D.TopRight}", Font);
                _g.DrawString(
                    $"{hSurface2D.TopRight}", Font, Brushes.LimeGreen, Membitrect.Right - 6 - sz.Width,
                     8);
            }
            if (hSurface2D.BottomLeft != null) {
                _g.DrawString(
                    $"{hSurface2D.BottomLeft}", Font, Brushes.LimeGreen, 6,
                     Membitrect.Top
                         + (Membitrect.Bottom - Membitrect.Top) - 26 - 8);
            }
            if (hSurface2D.BottomRight != null) {
                var sz = _g.MeasureString($"{hSurface2D.BottomRight}", Font);
                _g.DrawString(
                    $"{hSurface2D.BottomRight}", Font, Brushes.LimeGreen, Membitrect.Right - 6 - sz.Width,
                     Membitrect.Top
                         + (Membitrect.Bottom - Membitrect.Top) - 26 - 8);
            }

            /*
            if (hSurface2D.Title == null) {
                _g.DrawString(
                    $"{phase:n3}s", Font, Brushes.LimeGreen, 6, 6);
            } else {
                _g.DrawString(
                    $"{hSurface2D.Title}", Font, Brushes.LimeGreen, 6, 6);
            }*/

            _g.Dispose();
            int margin = 0;
            User32.StretchBlt(hdc, margin, margin, lprct.Right - lprct.Left - margin - margin,
                lprct.Bottom - lprct.Top - margin - margin, Memhdc, 0, 0,
                Membitrect.Right - Membitrect.Left - margin - margin,
                Membitrect.Bottom - Membitrect.Top - margin - margin,
                User32.TernaryRasterOperations.SRCCOPY);
            User32.DeleteObject(Membitmap);
            User32.DeleteDC(Memhdc);
            User32.EndPaint(hWnd, ref ps);
        }

        private static unsafe void OnWinMM(IntPtr hWnd, IntPtr wParam, IntPtr lParam) {
            User32.GetClientRect(hWnd, out RECT lprctw3);
            User32.InvalidateRect(hWnd, ref lprctw3, false);
        }

        public void Invalidate() {
            if (hWnd == IntPtr.Zero) {
                throw new ObjectDisposedException(GetType().Name);
            }
            User32.GetClientRect(hWnd, out RECT lprctw);
            User32.InvalidateRect(hWnd, ref lprctw, false);
        }
        public void Show() {
            if (hWnd == IntPtr.Zero) {
                throw new ObjectDisposedException(GetType().Name);
            }
            User32.ShowWindow(hWnd, ShowWindowCommands.Normal);
            User32.UpdateWindow(hWnd);
        }
    }
}