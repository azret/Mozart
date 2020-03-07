using System;
using System.Audio;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Microsoft.WinMM;

unsafe partial class App {
    Mic32 hMic32;

    Mic32 OpenMic() {
        var hMic32 = new Microsoft.WinMM.Mic32(1024, 44100, (hMic, hWaveHeader) => {
            WaveHeader* pwh = (WaveHeader*)hWaveHeader;
            if (pwh != null) {
                short* psData =
                    (short*)((*pwh).lpData);
                // hMic.CaptureData(pwh, psData);
                var f = 10 * ((float)44100 / (float)1024);
                f = 440;
                hMic.CaptureData(Process.Sine(f, hMic.Hz, hMic.Samples));
                var X = hMic.CH1();
                if (pwh != null) {
                    (*pwh).dwFlags = (*pwh).dwFlags & ~WaveHeaderFlags.Done;
                }
                WinMM.Throw(
                    WinMM.waveInAddBuffer(hMic.Handle, hWaveHeader, Marshal.SizeOf(typeof(WaveHeader))),
                    WinMM.ErrorSource.WaveIn);
                var fft = Complex.FFT(X);
                Stream.Push(fft);
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

    public void Mute() {
        Interlocked.Exchange(
            ref hMic32,
            null)?.Dispose();
    }

    public void UnMute() {
        Interlocked.Exchange(
            ref hMic32,
            OpenMic())?.Dispose();
    }
}