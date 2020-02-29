using System;
using System.Audio;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.WinMM;

unsafe partial class App {
    public string CurrentDirectory {
        get => Environment.CurrentDirectory.Trim('\\', '/');
        set => Environment.CurrentDirectory = value;
    }

    public readonly Mic32 Mic;

    public App() {
        Mic = OpenMic();
    }

    static Mic32 OpenMic() {
        int samples = 1024;
        var mic = new Microsoft.WinMM.Mic32(samples, Wav._hz, (hMic, hWaveHeader) => {
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
                hMic.Notify(hMic, hWaveHeader);
            }
        });
        try {
            mic.Open();
        } catch (Exception e) {
            Console.Error?.WriteLine(e);
            mic.Dispose();
            mic = null;
        }
        return mic;
    }

    static void Main() {
        runCli(new App());
    }
}