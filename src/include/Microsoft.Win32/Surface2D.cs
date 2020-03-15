namespace Microsoft.Win32.Plot2D {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    public struct Pixel2D {
        public float Magnitude;
        public Color Color;
        public Pixel2D(double value) {
            Magnitude = (float)value;
            Color = Color.Black;
        }
        public Pixel2D(float value) {
            Magnitude = value;
            Color = Color.Black;
        }
        public Pixel2D(float value, Color color) {
            Magnitude = value;
            Color = color;
        }
        public Pixel2D(double value, Color color) {
            Magnitude = (float)value;
            Color = color;
        }
    }

    public class Surface2D : IDisposable {
        public static int linear(float val, float from, float to) {
            return (int)(val * to / from);
        }
        public static float linf(float val, float from, float to) {
            return (val * to / from);
        }
        public struct Quantum {
            const float ALIGN = 4.0f;
            public readonly int Value;
            public Quantum(double value) {
                var val = (int)((int)(value / ALIGN) * ALIGN);
                Debug.Assert(val <= value);
                Value = val;
            }
            public Quantum(int value) : this((double)value) { }
        }
        public Bitmap hBitMap;
        BitmapData hData;
        public readonly int Width;
        public readonly int Height;
        public readonly Color _bgColor;
        public Surface2D(Quantum width, Quantum height, Color bgColor) {
            _bgColor = bgColor;
            Width = width.Value;
            Height = height.Value;
            hBitMap = (Width > 0 && Height > 0)
                ? new Bitmap(Width, Height,
                    PixelFormat.Format24bppRgb)
                : null;
        }
        public string TopLeft = null, TopRight = null,
            BottomLeft = null,
                BottomRight = null;
        public string Title = null;
        public void BeginPaint() {
            Title = null;
            hData = hBitMap?.LockBits(
                new Rectangle(0, 0, hBitMap.Width, hBitMap.Height),
                ImageLockMode.WriteOnly,
                hBitMap.PixelFormat
            );
        }
        public void EndPaint() {
            if (hData != null) {
                hBitMap?.UnlockBits(hData);
                hData = null;
            }
        }

        public void Dispose() {
            EndPaint();
            hBitMap?.Dispose();
        }

        public void Fill(Color bgColor) {
            if (hData == null) return;
            long U = Environment.TickCount;
            unsafe {
                int per = hData.Stride / hData.Width;
                Debug.Assert(3 == per);
                byte* pBuff = (byte*)hData.Scan0.ToPointer();
                int cbBuff = hData.Stride
                    * hData.Height;
                int i = 0;
                for (byte* p = pBuff; p < pBuff + cbBuff - 1;) {
                    byte R = bgColor.R, G = bgColor.G, B = bgColor.B;
                    var offst = (int)(((ulong)p - (ulong)pBuff) / 3);
                    int x = (offst % hData.Width);
                    int y = (offst / hData.Width);
                    U = U * 25214903917 + 11;
                    Debug.Assert(offst == i);
                    *p = B; /*B*/ p++;
                    *p = G; /*G*/ p++;
                    *p = R; /*R*/ p++;
                    i++;
                }
            }
        }

        public static Color ChangeColorBrightness(Color color, float correctionFactor) {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;
            if (correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            } else {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            if (red > 255) red = 255;
            if (green > 255) green = 255;
            if (blue > 255) blue = 255;
            if (red < 0) red = 0;
            if (green < 0) green = 0;
            if (blue < 0) blue = 0;
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        public void Plot(Func<float, Pixel2D?> F,
            float LEN, bool bBars = false, float INCR = 1.0f, float START = 0.0f,
            float NORM = 1.0f) {
            if (hData == null) return;
            long U = Environment.TickCount;
            unsafe {
                int per = hData.Stride / hData.Width;
                Debug.Assert(3 == per);
                byte* bBuff = (byte*)hData.Scan0.ToPointer();
                int cbBuff = hData.Stride
                    * hData.Height;
                void PIXEL(int x, int y, byte R = 0, byte G = 0, byte B = 0) {
                    int sample = x * per;
                    if ((sample >= 0 && sample < hData.Stride) &&
                            (y >= 0 && y < hData.Height)) {
                        int offst = (y * hData.Stride) + sample;
                        if (offst >= 0 && offst < cbBuff)
                            bBuff[offst] = B;
                        if (offst + 1 >= 0 && offst < cbBuff)
                            bBuff[offst + 1] = G;
                        if (offst + 2 >= 0 && offst < cbBuff)
                            bBuff[offst + 2] = R;
                    }
                }
                int M = (hData.Height / 2);
                void DOT(int x, int y, byte R = 0, byte G = 0, byte B = 0) {
                    PIXEL(x, y, R, G, B);
                    PIXEL(x, y - 1, R, G, B);
                    PIXEL(x, y + 1, R, G, B);
                    PIXEL(x - 1, y, R, G, B);
                    PIXEL(x + 1, y, R, G, B);
                    if (bBars && y < M) {
                        while (y < M) {
                            PIXEL(x, y, R, G, B);
                            PIXEL(x - 1, y, R, G, B);
                            PIXEL(x + 1, y, R, G, B);
                            y++;
                        }
                    } else if (bBars && y > M) {
                        while (y > M) {
                            PIXEL(x, y, R, G, B);
                            PIXEL(x - 1, y, R, G, B);
                            PIXEL(x + 1, y, R, G, B);
                            y--;
                        }
                    }
                }
                for (float I = START; I < LEN - START; I += INCR) {
                    var ret = F(I);
                    if (!ret.HasValue) {
                        continue;
                    }
                    int horz = (int)linear(I,
                       LEN,
                       hData.Width);
                    float ampl = ret.Value.Magnitude * (float)NORM;
                    if (ampl < -1) ampl = -1;
                    if (ampl > 1) ampl = 1;
                    if (ampl < -1 || ampl > +1) {
                        throw new IndexOutOfRangeException();
                    }
                    int vert = (int)linear(-ampl, 1, M) + M;
                    DOT(horz, vert,
                        ret.Value.Color.R, ret.Value.Color.G, ret.Value.Color.B);
                    U = U * 25214903917 + 11;
                }
            }
        }

        public void Plot<T>(Func<T, int, IEnumerable<Pixel2D?>> F, T[] data, int cc) {
            if (data == null) return;
            if (hData == null) return;
            long U = Environment.TickCount;
            unsafe {
                int per = hData.Stride / hData.Width;
                Debug.Assert(3 == per);
                byte* bBuff = (byte*)hData.Scan0.ToPointer();
                int cbBuff = hData.Stride * hData.Height;
                void SetPixel(int x, int y, byte R = 0, byte G = 0, byte B = 0) {
                    int sample = x * per;
                    if ((sample >= 0 && sample < hData.Stride) &&
                            (y >= 0 && y < hData.Height)) {
                        int offst = (y * hData.Stride) + sample;
                        if (offst >= 0 && offst < cbBuff)
                            bBuff[offst] = B;
                        if (offst + 1 >= 0 && offst < cbBuff)
                            bBuff[offst + 1] = G;
                        if (offst + 2 >= 0 && offst < cbBuff)
                            bBuff[offst + 2] = R;
                    }
                }
                void DOT(int x, int y, byte R = 0, byte G = 0, byte B = 0) {
                    SetPixel(x, y, R, G, B);
                    SetPixel(x, y - 1, R, G, B);
                    SetPixel(x, y + 1, R, G, B);
                    SetPixel(x - 1, y, R, G, B);
                    SetPixel(x + 1, y, R, G, B);
                    SetPixel(x - 1, y - 1, R, G, B);
                    SetPixel(x + 1, y + 1, R, G, B);
                    SetPixel(x - 1, y + 1, R, G, B);
                    SetPixel(x + 1, y - 1, R, G, B);
                }
                for (int i = 0; i < cc; i++) {
                    if (i >= data.Length) {
                        break;
                    }
                    int horz = linear(i, cc, Width);
                    Pixel2D?[] Q = F(data[i], i)?.ToArray();
                    if (Q == null) continue;
                    for (var k = 0; k < Q.Length; k++) {
                        var q = Q[k];
                        if (!q.HasValue) {
                            continue;
                        }
                        float ampl = q.Value.Magnitude;
                        if (ampl < 0) ampl = 0;
                        if (ampl > 1) ampl = 1;
                        if (ampl < 0 || ampl > 1) {
                            throw new IndexOutOfRangeException();
                        }
                        int vert = linear(ampl,
                            1,
                            hData.Height);
                        if (vert >= hData.Height) {
                            vert = hData.Height - 1;
                        }
                        DOT(horz, vert,
                            q.Value.Color.R, q.Value.Color.G, q.Value.Color.B);
                        U = U * 25214903917 + 11;
                    }
                }
            }
        }

        public void Dot(int ax, int ay, Color color) {
            if (hData == null) return;
            long U = Environment.TickCount;
            unsafe {
                int per = hData.Stride / hData.Width;
                Debug.Assert(3 == per);
                byte* bBuff = (byte*)hData.Scan0.ToPointer();
                int cbBuff = hData.Stride
                    * hData.Height;
                void PIXEL(int x, int y, byte R = 0, byte G = 0, byte B = 0) {
                    int sample = x * per;
                    if ((sample >= 0 && sample < hData.Stride) &&
                            (y >= 0 && y < hData.Height)) {
                        int offst = (y * hData.Stride) + sample;
                        if (offst >= 0 && offst < cbBuff)
                            bBuff[offst] = B;
                        if (offst + 1 >= 0 && offst < cbBuff)
                            bBuff[offst + 1] = G;
                        if (offst + 2 >= 0 && offst < cbBuff)
                            bBuff[offst + 2] = R;
                    }
                }
                PIXEL(ax, ay, color.R, color.G, color.B);
            }
        }

        public void Line(Func<int, int, Color> color, Func<int, int, double?> func, bool bBars = false) {
            if (hData == null) return;
            long U = Environment.TickCount;
            unsafe {
                int per = hData.Stride / hData.Width;
                Debug.Assert(3 == per);
                byte* bBuff = (byte*)hData.Scan0.ToPointer();
                int cbBuff = hData.Stride
                    * hData.Height;
                void SetPixel(int x, int y, byte R = 0, byte G = 0, byte B = 0) {
                    int sample = x * per;
                    if ((sample >= 0 && sample < hData.Stride) &&
                            (y >= 0 && y < hData.Height)) {
                        int offst = (y * hData.Stride) + sample;
                        if (offst >= 0 && offst < cbBuff)
                            bBuff[offst] = B;
                        if (offst + 1 >= 0 && offst < cbBuff)
                            bBuff[offst + 1] = G;
                        if (offst + 2 >= 0 && offst < cbBuff)
                            bBuff[offst + 2] = R;
                    }
                }
                int M = (int)((hData.Height) / 2);
                void DOT(int x, int y, byte R = 0, byte G = 0, byte B = 0) {
                    SetPixel(x, y, R, G, B);
                    if (!bBars) {
                        SetPixel(x, y - 1, R, G, B);
                        SetPixel(x, y + 1, R, G, B);
                        SetPixel(x - 1, y, R, G, B);
                        SetPixel(x + 1, y, R, G, B);
                    }
                    if (bBars && y < M) {
                        while (y < M) {
                            SetPixel(x, y, R, G, B);
                            // SetPixel(x - 1, y, R, G, B);
                            // SetPixel(x + 1, y, R, G, B);
                            y++;
                        }
                    } else if (bBars && y > M) {
                        while (y > M) {
                            SetPixel(x, y, R, G, B);
                            // SetPixel(x - 1, y, R, G, B);
                            // SetPixel(x + 1, y, R, G, B);
                            y--;
                        }
                    }
                }
                for (var x = 0; x < hData.Width; x++) {
                    var value = func(x, hData.Width);
                    if (!value.HasValue) {
                        continue;
                    }
                    double ampl = value.Value;
                    if (ampl < -1) ampl = -1;
                    if (ampl > 1) ampl = 1;
                    if (ampl < -1 || ampl > +1) {
                        throw new IndexOutOfRangeException();
                    }
                    int y = linear(-(float)ampl, 1, M) + M;
                    var c = color(x, hData.Width);
                    DOT(
                        x,
                        y,
                        c.R, c.G, c.B);
                }
            }
        }
    }
}
