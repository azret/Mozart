using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace System.Audio {
    public static partial class Wav {
        public static float[] Read(string fileName, out int hz) {
            float[] _aSamples = null;
            using (var file = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                if (file.ReadStr(4) != "RIFF") {
                    throw new InvalidDataException();
                }
                var nFileLen = file.ReadInt() + 8;
                if (file.ReadStr(8) != "WAVEfmt ") {
                    throw new InvalidDataException();
                }
                var nSubchunk = file.ReadInt();
                var AudioFormat = file.ReadShort();
                if (AudioFormat != 1) {
                    throw new InvalidDataException();
                }
                short _nChannels = file.ReadShort();
                int _nHz = file.ReadInt();
                hz = _nHz;
                Debug.Assert(_nHz == 44100);
                int _nBytesPerSec = file.ReadInt();
                var nBlkAlign = file.ReadShort();
                short _nBitsPerSample = file.ReadShort();
                var ExtraPadding = file.ReadStr(nSubchunk - 16);
                while (file.Position < file.Length) {
                    var cSection = file.ReadStr(4);
                    switch (cSection) {
                        case "fact":
                            var nFactchunk = file.ReadInt();
                            var nRealSize = file.ReadInt();
                            break;
                        case "data":
                            var nBytesData = file.ReadInt();
                            var nSamples = (int)(nBytesData / (_nBitsPerSample / 8));
                            _aSamples = new float[nSamples / _nChannels];
                            for (int i = 0; i < _aSamples.Length; i++) {
                                switch (_nBitsPerSample) {
                                    case 8:
                                        _aSamples[i] = file.ReadByte() - 128;
                                        // _aSamples[i].Right = _aSamples[i].Left;
                                        Debug.Assert(_nChannels == 1);
                                        break;
                                    case 16:
                                        var ch1 = file.ReadShort() / 32767.0f;
                                        _aSamples[i] = ch1;
                                        if (_nChannels == 2) {
                                            var ch2 = file.ReadShort() / 32767.0f;
                                        } else {
                                            Debug.Assert(_nChannels == 1);
                                        }
                                        break;
                                }
                            }
                            break;
                        case "LIST":
                            var nLISTBytes = file.ReadInt();
                            file.Position += nLISTBytes;
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                }
            }
            return _aSamples;
        }
        public static void Write(string fileName, IEnumerable<float[]> data, int Hz) {
            short _nChannels = 2,
                _nBitsPerSample = 16;
            using (FileStream file = System.IO.File.Create(fileName)) {
                var nFileLen = 0;
                var nSamples = 0;
                file.WriteString("RIFF");
                file.WriteInt(nFileLen);
                file.WriteString("WAVE");
                file.WriteString("fmt ");
                file.WriteInt(16);
                file.WriteShort((short)1);
                file.WriteShort((short)_nChannels);
                file.WriteInt(Hz);
                file.WriteInt(Hz * _nChannels * _nBitsPerSample / 8);
                file.WriteShort((short)(_nChannels * _nBitsPerSample / 8));
                file.WriteShort((short)_nBitsPerSample);
                file.WriteString("data");
                file.WriteInt((int)(_nChannels * nSamples * _nBitsPerSample / 8));
                foreach (float[] samples in data) {
                    for (int j = 0; j < samples.Length; j++) {
                        var cc = _nChannels;
                        while (cc-- > 0) {
                            var vol = samples[j];
                            switch (_nBitsPerSample) {
                                case 16:
                                    vol *= 32768;
                                    vol = System.Math.Max(System.Math.Min(vol, 32767), -32768);
                                    file.WriteShort((short)(vol));
                                    break;
                                default:
                                    throw new InvalidDataException();
                            }
                        }
                        nSamples++;
                    }
                }
                file.Seek(4, SeekOrigin.Begin);
                nFileLen = nSamples * _nBitsPerSample / 8 + 44;
                file.WriteInt(nFileLen);
                file.Seek(40, SeekOrigin.Begin);
                file.WriteInt((int)(_nChannels * nSamples * _nBitsPerSample / 8));
                file.Seek(0, SeekOrigin.End);
            }
        }
        static int ReadInt(this FileStream file) {
            var arr = new byte[4];
            file.Read(arr, 0, 4);
            return BitConverter.ToInt32(arr, 0);
        }
        static short ReadShort(this FileStream file) {
            return (short)(file.ReadByte() + (file.ReadByte() << 8));
        }
        static string ReadStr(this FileStream file, int max) {
            var bytes = new byte[max];
            file.Read(bytes, 0, max);
            return Encoding.ASCII.GetString(bytes);
        }
        static void WriteBytes(this FileStream file, byte[] value) {
            file.Write(value, 0, value.Length);
        }
        static void WriteString(this FileStream file, string value) {
            file.WriteBytes(Encoding.ASCII.GetBytes(value));
        }
        static void WriteInt(this FileStream file, int value) {
            file.WriteBytes(BitConverter.GetBytes(value));
        }
        static void WriteShort(this FileStream file, short value) {
            file.WriteBytes(BitConverter.GetBytes(value));
        }
    }
    public static partial class Wav {
        public static IEnumerable<TimeSpan> Parse(string aSz, string fmt) {
            var Model = ParseVector(fmt, aSz, "0.8", 0);
            yield return Model;
        }
        public static IEnumerable<TimeSpan> Parse(string inputFilePath) {
            string fmt = null;
            Console.Write($"\r\nReading from \"{inputFilePath}\"...\r\n\r\n");
            string[] lines = File.ReadAllLines(inputFilePath);
            var Model = Parse(ref fmt, lines);
            Console.Write($"Ready!\r\n\r\n");
            return Model;
        }
        public static IEnumerable<TimeSpan> Parse(ref string fmt, string[] lines) {
            var Model = new TimeSpan[lines.Length - 1];
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
        static TimeSpan ParseVector(string aFmt, string aSz) {
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
        static TimeSpan ParseVector(string aFmt, string aSz, string d, int i) {
            if (aFmt == "MIDI") {
            } else {
                throw new InvalidDataException("Invalid format.");
            }
            List<Frequency> aList;
            TimeSpan aIt = new TimeSpan(float.Parse(d),
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
                    var f = Frequency.Parse(Freq);
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