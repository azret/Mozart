using System;
using System.Collections;
using System.IO;
using System.Text;

static partial class App {
    public static void SaveMidi(Vector[] Model, string fmt, string outputFilePath) {
        SaveMidi(
            Model,
            fmt,
            outputFilePath,
            0,
            Model.Length);
    }
    public static void SaveMidi(Vector[] Model, string fmt, string outputFilePath,
        int offset, int len) {
        Console.Write($"\r\nSaving: {outputFilePath}...\r\n");
        using (var stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
            int i = 0;
            string s;
            s = $"{fmt} " +
                $"⁞ {System.Ai.CBOW.DIMS}\r\n";
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            stream.Write(bytes,
                0, bytes.Length);
            for (int sample = offset; sample < (offset + len); sample++) {
                Vector it = Model[sample];
                if (it == null) {
                    // s = "𝆕 ⁞ 0.00000±i0\r\n";
                    // bytes = Encoding.UTF8.GetBytes(s);
                    // stream.Write(
                    //     bytes,
                    //     0,
                    //     bytes.Length);
                    // i++;
                } else {
                    var line = new StringBuilder();
                    Complex[] axis = it.Axis;
                    if (axis != null) {
                        for (var j = 0; j < axis.Length; j++) {
                            var n = Envelopes.MIDI2NOTE(j);
                            if (string.IsNullOrWhiteSpace(n)) {
                                n = axis[j].Im.ToString();
                            }
                            if (axis[j].Re > 0) {
                                if (line.Length > 0) {
                                    line.Append(" ");
                                }
                                var dB = Envelopes.dB(axis[j].Re);
                                var sign = dB < 0
                                    ? "-"
                                    : "+";
                                line.Append(n + sign + "i" + Math.Abs(dB).ToString());
                            }
                        }
                    }
                    string score = it.Score.ToString();
                    if (line.Length > 0) {
                        s = $"{it.Id} ⁞ {score} ⁞ {line.ToString()}\r\n";
                    } else {
                        s = $"{it.Id} ⁞ {score}\r\n";
                    }
                    bytes = Encoding.UTF8.GetBytes(s);
                    stream.Write(
                        bytes,
                        0,
                        bytes.Length);
                    i++;
                }
            }
        }
        Console.Write("\r\nReady!\r\n");
    }
}