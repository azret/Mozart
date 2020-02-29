using System;
using System.Ai;
using System.Audio;
using System.Collections;
using System.IO;
using Microsoft.Win32;

static partial class App {
    static bool Md(
        string cliScript,
        Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--md")) {
            cliScript = cliScript.Remove(0, "--md".Length).Trim();
        } else if (cliScript.StartsWith("md")) {
            cliScript = cliScript.Remove(0, "md".Length).Trim();
        } else {
            throw new ArgumentException();
        }

        string inWavFile = Path.GetFullPath(cliScript);
        if (!File.Exists(inWavFile) 
            && string.IsNullOrWhiteSpace(Path.GetExtension(inWavFile))) {
            if (File.Exists(Path.ChangeExtension(inWavFile, ".wav"))) {
                inWavFile = Path.ChangeExtension(inWavFile, ".wav");
            }
        }

        var inWav = Wav.Read(inWavFile);

        Set filter = new Set(
            "C3"
        );

        Matrix Model = System.Ai.Mel.ShortTimeFourierTransform(inWav,
                filter,
                0.01,
                -20,
                +20);

        SaveMidi(
            Model.GetBuffer(),
            "MIDI",
            Path.ChangeExtension(inWavFile, ".md"));

        Model = System.Ai.Model.LoadFromFile(
            Path.ChangeExtension(inWavFile, ".md"),
            System.Ai.Cli.SIZE,
            out string fmt, out CBOW.DIMS);

        Console.Write($"\r\nSynthesizing: {Path.ChangeExtension(inWavFile, ".g.wav")}...\r\n");

        Wav.Write(
            Path.ChangeExtension(inWavFile, ".g.wav"),
            Wav.Synthesize(Model.GetBuffer())
        );

        StartWinUI(
            () => {
                return Model;
            },
            Path.ChangeExtension(inWavFile, ".g.wav")
        );

        WinMM.PlaySound(Path.ChangeExtension(inWavFile, ".g.wav"),
            IntPtr.Zero,
            WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
            WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
            WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
            WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
            WinMM.PLAYSOUNDFLAGS.SND_PURGE);

        return false;
    }

    static bool Noise(
        string cliScript,
        Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--noise")) {
            cliScript = cliScript.Remove(0, "--noise".Length).Trim();
        } else if (cliScript.StartsWith("noise")) {
            cliScript = cliScript.Remove(0, "noise".Length).Trim();
        } else {
            throw new ArgumentException();
        }

        string wavFile = "noise.wav";

        Set filter = new Set(
            "C3"
        );

        var Model = System.Ai.Mel.Noise();

        SaveMidi(
            Model.GetBuffer(),
            "MIDI",
            Path.ChangeExtension(wavFile, ".md"));

        // Model = LoadFromFile(Path.ChangeExtension(wavFile, ".md"),
        //     out string fmt, out CBOW.DIMS);

        Console.Write($"\r\nSynthesizing: {Path.ChangeExtension(wavFile, ".g.wav")}...\r\n");

        Wav.Write(
            Path.ChangeExtension(wavFile, ".g.wav"),
            Wav.Synthesize(Model.GetBuffer())
        );

        StartWinUI(
            () => {
                return Model;
            },
            Path.ChangeExtension(wavFile, ".g.wav")
        );

        WinMM.PlaySound(Path.ChangeExtension(wavFile, ".g.wav"),
            IntPtr.Zero,
            WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
            WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
            WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
            WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
            WinMM.PLAYSOUNDFLAGS.SND_PURGE);

        return false;
    }
}