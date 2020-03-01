using System;
using System.Audio;
using System.IO;

partial class App {
    static bool PlayFrequency(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        string title = cliScript;

        if (string.IsNullOrWhiteSpace(cliScript)) {
            return false;
        }

        var inFile = @"D:\data\play.md";

        Chord[] inWav = Wav.Parse(inFile);

        var wavOutFile = Path.GetFullPath(Path.ChangeExtension(inFile, ".g.wav"));

        Console.Write($"\r\nSynthesizing...\r\n\r\n");

        var data = Wav.Synthesize(inWav);

        Wav.Write(wavOutFile, data);

        Console.Write($"\r\nReady!\r\n\r\n");

        Microsoft.Win32.WinMM.PlaySound(wavOutFile,
            IntPtr.Zero,
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
            // Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_LOOP |
            Microsoft.Win32.WinMM.PLAYSOUNDFLAGS.SND_PURGE);

        return false;
    }
}