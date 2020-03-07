using System;
using System.Audio;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

partial class App {
    static bool ScoreWav(App app,
        string cliScript,
        Func<bool> IsTerminated) {
        string title = cliScript;

        cliScript = "Elise";

        if (string.IsNullOrWhiteSpace(cliScript)) {
            return false;
        }

        var inWavFile = Path.ChangeExtension(
            Path.Combine(app.CurrentDirectory, cliScript), ".wav");

        if (!File.Exists(inWavFile)) {
            throw new FileNotFoundException();
        }

        var inWav = Wav.Read(inWavFile, out int hz);

        var norm = 0d;
        for (int i = 0; i < inWav.Length; i++) {
            norm += Math.Abs(inWav[i]);
        }
        norm /= inWav.Length  * 2;
        norm = 1 / norm;
        if (norm > 0) {
            for (int i = 0; i < inWav.Length; i++) {
                inWav[i] *= ((float)norm);
            }
        }

        List<System.Audio.TimeSpan> music = new List<System.Audio.TimeSpan>();

        int samples = 1024;

        foreach (Complex[] fft in Complex.ShortTimeFourierTransform(
                inWav, samples, Shapes.Welch)) {
        
            var span = System.Audio.Process.Translate(
                fft, hz);

            var duration =
                    Math.Round((double)fft.Length / (double)hz, 4);

            music.Add(new System.Audio.TimeSpan((float)duration, span));

            Print.Dump(Console.Out, span, samples, hz);

        }

        var outWav = Wav.Synthesize(music, hz, Shapes.Welch);

        var outWavFile = Path.ChangeExtension(
            inWavFile, ".g.wav");

        Wav.Write(outWavFile, outWav, hz);

        Microsoft.Win32.WinMM.PlaySound(outWavFile,
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