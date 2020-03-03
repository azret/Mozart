using System.Collections.Generic;
using System.IO;

namespace System.Audio {
    public static partial class Wav {
        public static IEnumerable<Set> Parse(string aSz, string fmt) {
            var Model = ParseVector(fmt, aSz, "0.8", 0);
            yield return Model;
        }
        public static IEnumerable<Set> Parse(string inputFilePath) {
            string fmt = null;
            Console.Write($"\r\nReading from \"{inputFilePath}\"...\r\n\r\n");
            string[] lines = File.ReadAllLines(inputFilePath);
            var Model = Parse(ref fmt, lines);
            Console.Write($"Ready!\r\n\r\n");
            return Model;
        }
        public static IEnumerable<Set> Parse(ref string fmt, string[] lines) {
            var Model = new Set[lines.Length - 1];
            for (int i = 0; i < lines.Length; i++) {
                string sz = lines[i];
                if (string.IsNullOrWhiteSpace(sz)) {
                    continue;
                }
                if (i == 0) {
                    ParseHeader(sz, out fmt);
                } else {
                    Model[i - 1] = ParseVector(
                        fmt,
                        sz);
                }
            }
            return Model;
            void ParseHeader(string aSz, out string aFmt) {
                int hz = 44100;
                int i = 0, wordStart = i;
                while (i < aSz.Length && (aSz[i] != ' ' && aSz[i] != '|' && aSz[i] != '⁞')) {
                    i++;
                }
                aFmt = aSz.Substring(wordStart, i - wordStart);
                while (i < aSz.Length && (aSz[i] == ' '
                        || aSz[i] == '|' || '⁞' == aSz[i])) {
                    i++;
                }
                if (aFmt != "CLI" && aFmt != "MEL" && aFmt != "CBOW" && aFmt != "MIDI") {
                    throw new InvalidDataException();
                }
                int section = 0;
                for (; ; ) {
                    wordStart = i;
                    while (i < aSz.Length && (aSz[i] == '-' || aSz[i] == '+' || aSz[i] == 'E'
                            || aSz[i] == '.' || char.IsDigit(aSz[i]))) {
                        i++;
                    }
                    if (i > wordStart) {
                        string num = aSz.Substring(wordStart, i - wordStart);
                        switch (section) {
                            case 0:
                                hz = int.Parse(num);
                                break;
                            default:
                                throw new InvalidDataException();
                        }
                        while (i < aSz.Length && (aSz[i] == ' ' || aSz[i] == '|' || '⁞' == aSz[i])) {
                            if (aSz[i] == '|' || aSz[i] == '⁞') {
                                section++;
                            }
                            i++;
                        }
                    } else {
                        break;
                    }
                }
            }
        }
        public static Set ParseVector(string aFmt, string aSz) {
            if (aFmt == "MIDI") {
            } else {
                throw new InvalidDataException("Invalid format.");
            }
            List<Frequency> aList = new List<Frequency>();
            int i = 0, wordStart = i;
            while (i < aSz.Length && (aSz[i] == '\t' || aSz[i] == ' ' || aSz[i] == '-' || aSz[i] == '+' || aSz[i] == '±'
                            || aSz[i] == '•' || aSz[i] == '|' || aSz[i] == '⁞' || aSz[i] == '░' || aSz[i] == '║')) {
                i++;
            }
            string w = aSz.Substring(wordStart, i - wordStart);
            while (i < aSz.Length && (aSz[i] == '\t' || aSz[i] == ' ' || aSz[i] == '-' || aSz[i] == '+' || aSz[i] == '±'
                            || aSz[i] == '•' || aSz[i] == '|' || aSz[i] == '⁞' || aSz[i] == '░' || aSz[i] == '║')) {
                i++;
            }
            wordStart = i;
            while (i < aSz.Length && (aSz[i] == '.' || char.IsDigit(aSz[i]))) {
                i++;
            }
            string d = aSz.Substring(wordStart, i - wordStart);
            if (i < aSz.Length && (aSz[i] == 's')) {
                i++;
            }
            return ParseVector(aFmt, aSz, d, i);
        }
        public static Set ParseVector(string aFmt, string aSz, string d, int i) {
            if (aFmt == "MIDI") {
            } else {
                throw new InvalidDataException("Invalid format.");
            }
            List<Frequency> aList;
            Set aIt = new Set(float.Parse(d),
                    aList = new List<Frequency>());
            for (; ; ) {
                while (i < aSz.Length && (aSz[i] == '\t' || aSz[i] == ' ' || aSz[i] == '•' || aSz[i] == '|' || aSz[i] == '⁞' ||
                    aSz[i] == '░' || aSz[i] == '║')) {
                    i++;
                }
                int wordStart = i;
                while (i < aSz.Length && (aSz[i] == 'E'
                        || aSz[i] == 'A' || aSz[i] == 'B' || aSz[i] == 'C'
                        || aSz[i] == 'D' || aSz[i] == 'F' || aSz[i] == 'G'
                        || aSz[i] == '#'
                        || aSz[i] == '.' || char.IsDigit(aSz[i]))) {
                    i++;
                }
                string Freq = aSz.Substring(
                    wordStart,
                    i - wordStart);
                if (!string.IsNullOrWhiteSpace(Freq)) {
                    string dB = null;
                    int dir = +1;
                    if (i < aSz.Length && (aSz[i] == '+' || aSz[i] == '-' || aSz[i] == '+' || aSz[i] == '±')) {
                        if (aSz[i] == '-') {
                            dir = -1;
                        }
                        i++;
                    }
                    if (i < aSz.Length && (char.IsDigit(aSz[i]))) {
                        wordStart = i;
                        while (i < aSz.Length && (char.IsDigit(aSz[i]) || aSz[i] == '.')) {
                            i++;
                        }
                        dB = aSz.Substring(wordStart, i - wordStart);
                        while (i < aSz.Length && (aSz[i] == 'D' || aSz[i] == 'd' || aSz[i] == 'b' || aSz[i] == 'B')) {
                            i++;
                        }
                    }
                    var f = Midi.Parse(Freq);
                    if (string.IsNullOrWhiteSpace(dB)) {
                        dB = "0";
                    }
                    aList.Add(new Frequency(
                        (float)f,
                        (float)System.Audio.dB.ToAmplitude((int)(dir * double.Parse(dB)))));
                } else /* End of Line */ {
                    break;
                }
            }
            return aIt;
        }
    }
}