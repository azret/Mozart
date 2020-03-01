using System;
using System.Audio;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Microsoft.WinMM;

unsafe partial class App {
    public string CurrentDirectory {
        get => Environment.CurrentDirectory.Trim('\\', '/');
        set => Environment.CurrentDirectory = value;
    }

    readonly Spectro Spectro = new Spectro();

    Timer _hTimer;

    public Mic32 Mic;

    public App() {
        _hTimer = new Timer((state) => {
            Loop();
        }, null, 0, 100);

        // Interlocked.Exchange(ref Mic,
        //     OpenMic())?.Dispose();
    }

    Mic32 OpenMic() {
        int samples = 1024;
        var hMic32 = new Microsoft.WinMM.Mic32(samples, Wav._hz, (hMic, hWaveHeader) => {
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
                Notify(hMic, hWaveHeader);
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

    static void Main() {
        runCli(new App());
    }
}