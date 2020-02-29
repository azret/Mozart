using System;
using System.Ai;
using System.Audio;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

static partial class App {
    static bool Play(
        string cliScript,
        Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--play")) {
            cliScript = cliScript.Remove(0, "--play".Length).Trim();
        } else if (cliScript.StartsWith("play")) {
            cliScript = cliScript.Remove(0, "play".Length).Trim();
        } else {
            throw new ArgumentException();
        }

        string md = cliScript;
        if (!File.Exists(md)
                    && string.IsNullOrWhiteSpace(Path.GetExtension(md))) {
            if (File.Exists(Path.ChangeExtension(md, ".md"))) {
                md = Path.ChangeExtension(md, ".md");
            }
        }

        var Model =
            File.Exists(md)
            ? System.Ai.Model.LoadFromFile(md, System.Ai.Cli.SIZE, out string fmt, out int dims)
            : BuildFromFragment(md);

        Console.Write($"Synthesizing: {Path.ChangeExtension(md, ".g.wav")}...\r\n");

        Wav.Write(Path.ChangeExtension(md, ".g.wav"),
            Wav.Synthesize(Model.GetBuffer()));

        StartWinUI(() => { return Model; }, Path.ChangeExtension(md, ".g.wav"));

        WinMM.PlaySound(Path.ChangeExtension(md, ".g.wav"),
            IntPtr.Zero,
            WinMM.PLAYSOUNDFLAGS.SND_ASYNC |
            WinMM.PLAYSOUNDFLAGS.SND_FILENAME |
            WinMM.PLAYSOUNDFLAGS.SND_NODEFAULT |
            WinMM.PLAYSOUNDFLAGS.SND_NOWAIT |
            WinMM.PLAYSOUNDFLAGS.SND_PURGE);

        return false;
    }
}