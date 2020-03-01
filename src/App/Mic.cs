using System;
using System.Audio;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.WinMM;

unsafe partial class App {
    public Mic32 Mic;

    Mic32 OpenMic() {
        var hMic32 = new Microsoft.WinMM.Mic32(Stream.Samples, Wav._hz, (hMic, hWaveHeader) => {
            WaveHeader* pwh = (WaveHeader*)hWaveHeader;
            if (pwh != null) {
                short* psData =
                    (short*)((*pwh).lpData);
                hMic.CaptureData(pwh, psData);
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
            hMic32.Open();
        } catch (Exception e) {
            Console.Error?.WriteLine(e);
            hMic32.Dispose();
            return null;
        }
        return hMic32;
    }
}