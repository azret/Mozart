using System.Collections;
using System.IO;
using System.Text;

namespace System.Ai {
    public static partial class Model {
        public static void SaveToFile(Vector[] Model, string fmt, int dims, string outputFilePath) {
            SaveToFile(
                Model,
                fmt,
                dims,
                outputFilePath,
                0,
                Model.Length);
        }
        public static void SaveToFile(Vector[] Model, string fmt, int dims, string outputFilePath,
            int offset, int len) {
            Console.Write($"\r\nSaving: {outputFilePath}...\r\n");
            using (var stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                int i = 0;
                string s;
                s = $"{fmt} " +
                    $"| {dims}\r\n";
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                stream.Write(bytes,
                    0, bytes.Length);
                for (int sample = offset; sample < (offset + len); sample++) {
                    Vector it = Model[sample];
                    if (it == null) {
                        continue;
                    }
                    var sb = new StringBuilder();
                    Complex[] axis = it.Axis;
                    if (axis != null) {
                        for (var j = 0; j < axis.Length; j++) {
                            if (fmt == "MIDI") {
                                if (axis[j].Re <= 0) {
                                    continue;
                                }
                            }
                            if (sb.Length > 0) {
                                sb.Append(" ");
                            }
                            sb.Append(axis[j]);
                        }
                    }
                    var score = it.z.ToString();
                    if (sb.Length > 0) {
                        s = $"{it.Id} | {score} | {sb.ToString()}\r\n";
                    } else {
                        s = $"{it.Id} | {score}\r\n";
                    }
                    bytes = Encoding.UTF8.GetBytes(s);
                    stream.Write(bytes,
                        0, bytes.Length);
                    i++;
                }
            }
            Console.Write("\r\nReady!\r\n");
        }
#if load
        public static Matrix LoadFromFile(string inputFilePath, int size, out string fmt, out int dims) {
            Matrix Model = new Matrix(size);
            fmt = null;
            Console.Write($"\r\nReading: {inputFilePath}...\r\n\r\n");
            string[] lines = File.ReadAllLines(inputFilePath);
            dims = 0;
            for (int i = 0; i < lines.Length; i++) {
                string l = lines[i];
                if (string.IsNullOrWhiteSpace(l)) {
                    continue;
                }
                if (i == 0) {
                    ParseHeader(l, out fmt, out dims);
                } else {
                    ParseVector(Model, fmt, l, dims);
                }
            }
            Console.Write($"Ready!\r\n\r\n");
            return Model;
        }
        static void ParseHeader(string sz, out string fmt, out int dims) {
            dims = -1;
            int i = 0, wordStart = i;
            while (i < sz.Length && (sz[i] != ' ' && sz[i] != '|' && sz[i] != '⁞')) {
                i++;
            }
            fmt = sz.Substring(wordStart, i - wordStart);
            while (i < sz.Length && (sz[i] == ' '
                    || sz[i] == '|' || '⁞' == sz[i])) {
                i++;
            }
            if (fmt != "CLI" && fmt != "MEL" && fmt != "CBOW" && fmt != "MIDI") {
                throw new InvalidDataException();
            }
            int section = 0;
            for (; ; ) {
                wordStart = i;
                while (i < sz.Length && (sz[i] == '-' || sz[i] == '+' || sz[i] == 'E'
                        || sz[i] == '.' || char.IsDigit(sz[i]))) {
                    i++;
                }
                if (i > wordStart) {
                    string num = sz.Substring(wordStart, i - wordStart);
                    switch (section) {
                        case 0:
                            dims = int.Parse(num);
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                    while (i < sz.Length && (sz[i] == ' ' || sz[i] == '|' || '⁞' == sz[i])) {
                        if (sz[i] == '|' || sz[i] == '⁞') {
                            section++;
                        }
                        i++;
                    }
                } else {
                    break;
                }
            }
        }
        static void ParseVector(Matrix Model, string fmt, string sz, int dims) {
            int i = 0, wordStart = i;
            while (i < sz.Length && (sz[i] != '\t' && sz[i] != ' '
                            && sz[i] != '•' && sz[i] != '|' && sz[i] != '⁞')) {
                i++;
            }
            string w = sz.Substring(wordStart, i - wordStart);
            while (i < sz.Length && (sz[i] == '\t' || sz[i] == ' '
                            || sz[i] == '•' || sz[i] == '|' || sz[i] == '⁞')) {
                i++;
            }
            Vector vec;
            if (fmt == "MIDI") {
                int t = Model.Count;
                if (t >= Model.Capacity) {
                    throw new OutOfMemoryException();
                }
                vec = new Vector(w);
                if (vec.Axis == null) {
                    vec.Axis = new Complex[dims];
                }
                Model[t] = vec;
            } else if (fmt == "CBOW" || fmt == "MEL" || fmt == "CLI") {
                vec = Model.Push(w);
                if (vec.Axis == null) {
                    vec.Axis = new Complex[dims];
                }
            } else {
                throw new InvalidDataException();
            }
            int section = 0,
                    n = 0;
            if (fmt == "MIDI") {
                n = dims;
            }
            for (; ; ) {
                wordStart = i;
                while (i < sz.Length && (sz[i] == '±' || sz[i] == '-' || sz[i] == '+' || sz[i] == 'E'
                        || sz[i] == 'A' || sz[i] == 'B' || sz[i] == 'C'
                        || sz[i] == 'D' || sz[i] == 'F' || sz[i] == 'G'
                        || sz[i] == '#'
                        || sz[i] == '.' || char.IsDigit(sz[i]))) {
                    i++;
                }
                var ImSign = +1;
                int len = (i - wordStart) - 1;
                while (wordStart + len > 0 && wordStart + len < sz.Length
                        && (sz[wordStart + len] == '-' || sz[wordStart + len] == '+' || sz[wordStart + len] == '±')) {
                    if (sz[wordStart + len] == '-') {
                        ImSign = -1;
                    }
                    len--;
                }
                string Re = sz.Substring(wordStart, len + 1);
                string Im = null;
                if (i < sz.Length && (sz[i] == 'i')) {
                    i++;
                    wordStart = i;
                    while (i < sz.Length && (sz[i] == '-' || sz[i] == '+' || sz[i] == 'E'
                            || sz[i] == 'A' || sz[i] == 'B' || sz[i] == 'C'
                            || sz[i] == 'D' || sz[i] == 'F' || sz[i] == 'G'
                            || sz[i] == '#'
                            || sz[i] == '.' || char.IsDigit(sz[i]))) {
                        i++;
                    }
                    Im = sz.Substring(wordStart, i - wordStart);
                }
                if (!string.IsNullOrWhiteSpace(Re)) {
                    switch (section) {
                        case 0:
                            vec.Score.Re = double.Parse(Re);
                            if (!string.IsNullOrWhiteSpace(Im)) {
                                vec.Score.Im = ImSign * double.Parse(Im);
                            }
                            break;
                        case 1:
                            if (fmt == "MIDI") {
                                var f = Envelopes.NOTE2FREQ(Re);
                                int m = Envelopes.FREQ2MIDI(f);
                                if (m >= 0 && m < vec.Axis.Length) {
                                    vec.Axis[m].Im = f;
                                    vec.Axis[m].Re = 1;
                                    if (!string.IsNullOrWhiteSpace(Im)) {
                                        vec.Axis[m].Re = Envelopes.Amplitude((int)(ImSign * double.Parse(Im)));
                                    }
                                }
                            } else {
                                vec.Axis[n].Re = double.Parse(Re);
                                if (!string.IsNullOrWhiteSpace(Im)) {
                                    vec.Axis[n].Im = ImSign * double.Parse(Im);
                                }
                                n++;
                            }
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                    while (i < sz.Length && (sz[i] == '\t' || sz[i] == ' ' || sz[i] == '•' || sz[i] == '|' || sz[i] == '⁞')) {
                        if (sz[i] == '•' || sz[i] == '|' || sz[i] == '⁞') {
                            section++;
                        }
                        i++;
                    }
                } else /* End of Line */ {
                    if (n != dims) {
                        throw new InvalidDataException();
                    }
                    break;
                }
            }
        }
#endif
    }
}