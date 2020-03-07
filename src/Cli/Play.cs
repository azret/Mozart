using System;
using System.Audio;
using System.Diagnostics;
using System.IO;
using System.Linq;

partial class App {
    static bool PlayFrequency(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        string title = cliScript;

        if (string.IsNullOrWhiteSpace(cliScript)) {
            return false;
        }

        var inWavFile = Path.ChangeExtension(
            Path.Combine(app.CurrentDirectory, cliScript), ".md");

        var inWav = File.Exists(inWavFile)
            ? Wav.Parse(inWavFile)
            : Wav.Parse(cliScript, "MIDI");

        var wavOutFile = File.Exists(inWavFile)
            ? Path.GetFullPath(Path.ChangeExtension(inWavFile, ".g.wav"))
            : Path.GetFullPath(Path.ChangeExtension("cli.wav", ".g.wav"));

        Console.Write($"\r\nSynthesizing...\r\n\r\n");

        var dataOut = Wav.Synthesize(inWav);

        Wav.Write(wavOutFile, dataOut);

        var dataIn = Wav.Read(wavOutFile)
            .Select(s => s.Left).ToArray();

        using (TextWriter writer = new StreamWriter(Path.ChangeExtension(inWavFile, ".g.md"))) {
            writer.WriteLine("MIDI");
            foreach (var fft in Complex.ShortTimeFourierTransform(
                            dataIn, 1024 * 4, Shapes.Hann)) {
                var span = System.Audio.Process.Translate(
                    fft, Stereo.Hz);
                Print.Dump(writer, span, 1024 * 4, Stereo.Hz);
            }
        }

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