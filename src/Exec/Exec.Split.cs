using System;
using System.Audio;
using System.Collections;
using System.IO;

static partial class App {
    static bool SplitSoundFragments(
        Session session,
        string run,
        Func<bool> IsTerminated) {
        if (run.StartsWith("--run")) {
            run = run.Remove(0, "--run".Length).Trim();
        } else if (run.StartsWith("run")) {
            run = run.Remove(0, "run".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        string wavInFile = Path.GetFullPath(run);
        var wavIn = Wav.Read(wavInFile);
        string outputFullPath = Path.GetFullPath(Path.GetFileNameWithoutExtension(Path.Combine(Path.GetFullPath(wavInFile))));
        if (Directory.Exists(outputFullPath)) {
            Directory.Delete(outputFullPath, true);
        }
        var Model = System.Ai.Mel.ShortTimeFourierTransform(
            wavIn,
            null
        );
        int startPosition = 0;
        int word = 0;
        for (int i = 0; i < Model.Count; i++) {
            var vec = Model[i];
            if (vec == null) {
                continue;
            }
            double Energy(Vector src) {
                var ENERGY = 0.0;
                for (var j = 0; j < vec.Axis.Length; j++) {
                    ENERGY += src.Axis[j].Re;
                }
                return ENERGY;
            }
            var EnergyInThisOneSample = Energy(vec);
            if (EnergyInThisOneSample == 0) {
                int len = i - startPosition;
                if (len > 0) {
                    var ENERGY_IN_SEGMENT = 0.0;
                    for (int sample = startPosition; sample < (startPosition + len); sample++) {
                        var it = Model[sample];
                        if (it == null) {
                            continue;
                        }
                        ENERGY_IN_SEGMENT += Energy(it);
                    }
                    if (ENERGY_IN_SEGMENT > 0) {
                        string outputFullFileName = Path.Combine(outputFullPath, $"{word}.md");
                        if (!Directory.Exists(Path.GetDirectoryName(outputFullFileName))) {
                            Directory.CreateDirectory(Path.GetDirectoryName(outputFullFileName));
                        }
                        var data = Model.GetBuffer();
                        Wav.Write(Path.ChangeExtension(outputFullFileName, ".g.wav"),
                            Wav.Synthesize(data,
                                startPosition,
                                len));
                        SaveMidi(
                            data,
                            "MIDI",
                            outputFullFileName,
                            startPosition,
                            len);
                        word++;
                    }
                }
                startPosition = i;
            }
        }
        return false;
    }
}