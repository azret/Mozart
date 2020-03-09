using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.Ai {

    public static partial class CBOW {
        public const int SIZE = 1048576,
            GENS = (int)1e6,
                SHUFFLE = (int)1e7;

        public static Word[] RunFullCosineSort(IOrthography lex, Matrix<Word> Model, string Q, int max) {
            if (Model == null || string.IsNullOrWhiteSpace(Q)) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Model not loaded.\r\n");
                Console.ResetColor();
                Console.WriteLine("See '--load' command for more info...\r\n");
                return null;
            }
            float[] Re = new float[CBOW.DIMS];
            float norm = 0;
            var sign = +1;
            foreach (var tok in PlainText.ForEach(Q, 0, Q.Length, 0)) {
                string wi = lex.GetKey(tok.TextFragment.Substring(tok.StartIndex, tok.Length));
                if (wi == "+") {
                    sign = +1;
                } else if (wi == "-") {
                    sign = -1;
                } else {
                    var vec = Model[wi];
                    if (vec != null) {
                        Debug.Assert(vec.Elements.Length == Re.Length);
                        for (var j = 0; j < Re.Length; j++) {
                            Re[j] += sign * vec.Elements[j].Re;
                        }
                        norm++;
                    } else {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"'{wi}' not found.");
                        Console.ResetColor();
                    }
                }
            }
            if (norm > 0) {
                for (var j = 0; j < Re.Length; j++) {
                    Re[j] /= (float)norm;
                }
            }
            Word[] output = CBOW.Predict(Model, Re, max);
            Array.Sort(output,
                (a, b) => Dot.CompareTo(a, b));
            Console.WriteLine();
            Console.WriteLine(" [" + string.Join(",", Re.Select(re => Math.Round(re, 4)).Take(7)) + "...]");
            Console.WriteLine();
            int len = 0;
            for (int i = output.Length - 1; i >= 0; i--) {
                Word n = output[i];
                if (n != null) {
                    string str = n.Id;
                    var it = Model[n.Id];
                    if (it != null) {
                        // if (it.Count > 0) {
                        //     var best = it.ArgMax();
                        //     if (best != null) {
                        //         str = best.Id;
                        //     }
                        // }
                    }
                    if (len + str.Length > 37  /* break like if does not fit */) {
                        Console.WriteLine(
                            output.Length <= 31
                                ? $" {str} : {n.ToString(z: true)}"
                                : $" {str}");
                        len = 0;
                    } else {
                        Console.Write(
                            output.Length <= 31
                                ? $" {str} : {n.ToString(z: true)}"
                                : $" {str}");
                        len += str.Length;
                    }
                }
            }
            Console.WriteLine();
            return output;
        }

        public static bool Train(string currentDirectory, string searchPattern,
            Func<bool> IsTerminated) {

            currentDirectory = @"D:\Mozart\src\";

            var lex = new CSharp();

            string outputFileName = @"D:\Mozart\src\App.cbow";

            Matrix<Word> Model = null;

            if (File.Exists(outputFileName)) {
                Model = LoadFromFile(outputFileName,
                    SIZE, out string fmt, out int dims);
            } else {
                Model = BuildFromPlainText(currentDirectory,
                    "*.cs", lex, outputFileName);
            }

            TrainMikolovModel(MakeFileList(new string[] { currentDirectory },
                "*.cs", SearchOption.AllDirectories),
                lex,
                Model,
                (loss) => { },
                IsTerminated);

            CBOW.SaveToFile(
                Matrix<Word>.Sort(Model),
                "CBOW",
                CBOW.DIMS,
                outputFileName);

            return false;
        }

        static void TrainMikolovModel(Set sourceFiles, IOrthography lex,
            Matrix<Word> Model, Action<double> SetLoss, Func<bool> HasCtrlBreak) {
            if (Model == null) {
                Console.WriteLine("Model not loaded.");
                return;
            }
            Word[] negDistr = System.Ai.CBOW.CreateNegDistr(
                Model, SHUFFLE);
            Thread[] threads = new Thread[Environment.ProcessorCount * 2];
            int numberOfThreads = 0,
                verbOut = 0;
            for (var t = 0; t < threads.Length; t++) {
                threads[t] = new Thread(() => {
                    Interlocked.Increment(ref numberOfThreads);
                    try {
                        for (int iter = 0; iter < GENS; iter++) {
                            if (HasCtrlBreak != null && HasCtrlBreak()) {
                                break;
                            }
                            string[] Shuffle = ((IEnumerable<string>)sourceFiles).ToArray();
                            global::Random.Shuffle(Shuffle, Shuffle.Length);
                            foreach (string file in Shuffle) {
                                if (HasCtrlBreak != null && HasCtrlBreak()) {
                                    return;
                                }
                                try {
                                    Console.Write($"\r\nReading {file}...\r\n");
                                    var textFragment = File.ReadAllText(file);
                                    string[] slidingWindow
                                        = new string[2 * System.Ai.CBOW.WINDOW + 1];
                                    foreach (var q
                                            in PlainText.ForEach(textFragment, 0, textFragment.Length, 1 + (slidingWindow.Length >> 1))) {
                                        if (HasCtrlBreak != null && HasCtrlBreak()) {
                                            return;
                                        }
                                        var vocab = q.Type == PlainTextTag.TAG
                                            ? lex.GetKey(textFragment.Substring(
                                                q.StartIndex,
                                                q.Length))
                                            : null;
                                        for (int i = 0; i < slidingWindow.Length; i++) {
                                            if (i == slidingWindow.Length - 1) {
                                                slidingWindow[i] = vocab;
                                            } else {
                                                slidingWindow[i] = slidingWindow[i + 1];
                                            }
                                        }
                                        SetLoss(System.Ai.CBOW.learnWindow(Model,
                                            negDistr, slidingWindow,
                                            iter,
                                            HasCtrlBreak, ref verbOut));
                                    }
                                    Thread.Sleep(3000 + global::Random.Next(3000));
                                } finally {
                                }
                            }
                        }
                    } finally {
                        Interlocked.Decrement(ref numberOfThreads);
                    }
                    Console.Write($"[{Thread.CurrentThread.ManagedThreadId}] stopped...\r\n");
                });
            }
            foreach (var t in threads) { t.Start(); }
            foreach (var t in threads) {
                t.Join();
            }
            Debug.Assert(numberOfThreads == 0);
        }

        static Matrix<Word> BuildFromPlainText(string sourcePath, string searchPattern, IOrthography lex, string outputFileName) {
            var Model = new Matrix<Word>((id, hashCode) => new Word(id, hashCode), SIZE);

            Set SourceFiles = null,
                Black = null;

            var ignoreFile = Path.ChangeExtension(outputFileName, ".ignore");

            if (File.Exists(ignoreFile)) {
                Black = MakeBlackList(13452, File.ReadAllText(ignoreFile), lex);
            }

            ParsePlainTextFiles(
                Model,
                SourceFiles = MakeFileList(new string[] { sourcePath },
                    searchPattern, SearchOption.AllDirectories),
                lex,
                Black);

            Matrix<Word> White = null;

            var file = Path.ChangeExtension(outputFileName, ".allow");

            if (File.Exists(file)) {
                // White = MakeWhiteList(file, lex, SIZE);
            }

            if (White?.Count > 0 || CBOW.THRESHOLD > 0) {
                LimitToThreshold(White, ref Model);
            }

            InitializeAndRandomize(Model);

            return Model;
        }

        static void ParsePlainTextFiles(Matrix<Word> Model, Set files, IOrthography lex, Set skipList) {
            bool IsStopWord(string w) {
                return skipList != null
                    ? (skipList[w] != null)
                    : false;
            }
            foreach (string file in (IEnumerable<string>)files) {
                Console.Write($"Reading {file}...\r\n");
                string textFragment = File.ReadAllText(file);
                foreach (var t
                         in PlainText.ForEach(textFragment, 0, textFragment.Length, 0)) {
                    if (t.Type == PlainTextTag.TAG) {
                        var id = lex.GetKey(t.TextFragment.Substring(t.StartIndex, t.Length));
                        if (!IsStopWord(id)) {
                            var it = Model.Push(id);
                            it.Add(1f / CBOW.THRESHOLD);
                        }
                    }
                }
            }
        }

        static Set MakeBlackList(int hashSize, string textFragment, IOrthography lex) {
            var S = new Set();
            if (textFragment != null) {
                foreach (var t in PlainText.ForEach(textFragment, 0, textFragment.Length, 0)) {
                    if (t.Type == PlainTextTag.TAG) {
                        S.Push(lex.GetKey(t.TextFragment.Substring(t.StartIndex, t.Length)));
                    }
                }
            }
            return S;
        }

        static Matrix<Word> MakeWhiteList(string file, IOrthography lex, int hashSize) {
            var M = new Matrix<Word>((id, hashCode) => new Word(id, hashCode), hashSize);
            Console.Write($"\r\nReading {file}...\r\n\r\n");
            string textFragment = File.ReadAllText(file);
            foreach (var t in PlainText.ForEach(textFragment, 0, textFragment.Length, 0)) {
                if (t.Type == PlainTextTag.TAG) {
                    M.Push(lex.GetKey(t.TextFragment.Substring(t.StartIndex, t.Length)));
                }
            }
            return M;
        }

        static Set MakeFileList(string[] paths, string searchPattern, SearchOption searchOption) {
            var M = new Set();
            foreach (var path in paths) {
                FileAttributes attr = FileAttributes.Offline;
                try {
                    attr = File.GetAttributes(path);
                } catch (System.IO.FileNotFoundException) {
                }
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                    foreach (var s in Directory.EnumerateFiles(path, "*.*", searchOption)) {
                        string file = Path.GetFullPath(s).ToLowerInvariant();
                        if (M.Contains(file)) {
                            continue;
                        }
                        if (searchPattern != null
                                        && searchPattern != "*.*") {
                            if (!searchPattern.Contains(Path.GetExtension(file))) {
                                continue;
                            }
                        }
                        attr = File.GetAttributes(file);
                        if ((attr & FileAttributes.Hidden) != FileAttributes.Hidden) {
                            M.Push(file);
                        }
                    }
                } else {
                    string file = Path.GetFullPath(path).ToLowerInvariant();
                    attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.Hidden) != FileAttributes.Hidden) {
                        M.Push(file);
                    }
                }
            }
            return M;
        }

        static void LimitToThreshold(Matrix<Word> White, ref Matrix<Word> Model) {
            var Copy = new Matrix<Word>((id, hashCode) => new Word(id, hashCode), SIZE);
            foreach (var h in Model) {
                if (h.Re < 1) {
                    continue;
                }
                if (White != null) {
                    if (White[h.Id] == null) {
                        continue;
                    }
                }
                Word l = Copy.Push(
                    h.Id,
                    h.HashCode);
                l.Re = h.Re;
                l.Im = h.Im;
            }
            Model = Copy;
        }

        static void InitializeAndRandomize(Matrix<Word> Model) {
            double norm = 0;
            foreach (var w in Model) {
                w.Im = /* Just to break the mosaic pattern of the random number generator... */
                    ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                w.Alloc(System.Ai.CBOW.DIMS);
                for (int i = 0; i < w.Elements.Length; i++) {
                    w.Elements[i].Re = ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                    w.Elements[i].Im = ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                }
                w.Im = 0;
                if (w.Re > 0) {
                    w.Re = (float)Math.Round(w.Re);
                } else {
                    w.Re = 0;
                }
                norm = Math.Max(norm,
                    w.Re);
            }
            if (norm != 0) {
                foreach (var w in Model) {
                    w.Im = w.Re / (float)norm;
                }
            }
        }

        public static double PowScale(double score) {
            const double POW = 0.7351;
            const int Xmax = 100;
            if (score <= 1) {
                return 1;
            } else if (score >= Xmax) {
                return System.Math.Pow(Xmax, POW);
            } else {
                return System.Math.Pow(score, POW);
            }
        }

        public const int VERBOSITY = 13;
        public static class Defaults {
            public const double lr = 0.0371;
            public const int WINDOW = 7,
                NEGATIVES = 3;
            public const int DIMS = 37,
                THRESHOLD = 1;
        }
        public static double lr = Defaults.lr;
        public static int WINDOW = Defaults.WINDOW,
            NEGATIVES = Defaults.NEGATIVES;
        public static int DIMS = Defaults.DIMS,
            THRESHOLD = Defaults.THRESHOLD;

        public static Word[] Predict(IEnumerable<Word> Model, float[] Re, int max) {
            Word[] best = new Word[max];
            foreach (Word c in Model) {
                int b = 0;
                for (int j = 0; j < best.Length; j++) {
                    if (best[j] == null) {
                        b = j;
                        break;
                    }
                    if (best[j].Re < best[b].Re) {
                        b = j;
                    }
                }
                float dot = 0,
                    score;
                for (int j = 0; j < Re.Length; j++) {
                    dot += c.Elements[j].Im * Re[j];
                }
                score = (float)Sigmoid.f(dot);
                if (best[b] == null || best[b].Re < score) {
                    best[b] = new Word(c.Id, c.HashCode) {
                        Re = score
                    };
                }
            }
            return best;
        }

        public static string MemSize(double byteCount) {
            string size = "0 Bytes";
            if (byteCount >= 1073741824.0)
                size = String.Format("{0:##.##}", byteCount / 1073741824.0) + " GB";
            else if (byteCount >= 1048576.0)
                size = String.Format("{0:##.##}", byteCount / 1048576.0) + " MB";
            else if (byteCount >= 1024.0)
                size = String.Format("{0:##.##}", byteCount / 1024.0) + " KB";
            else if (byteCount > 0 && byteCount < 1024.0)
                size = byteCount.ToString() + " Bytes";
            return size;
        }

        public static Word[] CreateNegDistr(Matrix<Word> M, int size) {
            Console.Write($"\r\nCreating negative samples...\r\n");
            double norm = 0,
                    cc = 0;
            foreach (Word g in M) {
                norm += PowScale(g.Re);
                cc++;
            }
            if (norm > 0) {
                norm = 1 / norm;
            }
            Word[] negDistr = new Word[0]; int count = 0;
            foreach (Word g in M) {
                double samples = PowScale(g.Re) * size * norm;
                for (int j = 0; j < samples; j++) {
                    if (count >= negDistr.Length) {
                        Array.Resize(ref negDistr, (int)((negDistr.Length + 7) * 1.75));
                    }
                    negDistr[count++] = g;
                }
            }
            Array.Resize(ref negDistr, count);
            global::Random.Shuffle(
                negDistr,
                negDistr.Length);
            Console.Write($"\r\n  Size: {count}\r\n  Grams: {M.Count}\r\n  Mem: {MemSize(count * Marshal.SizeOf<IntPtr>())}\r\n\r\n");
            return negDistr;
        }

        static Word PickFromNegDistr(Word[] negDistr, Word vec) {
            if (negDistr == null) {
                return null;
            }
            int i = global::Random.Next(negDistr.Length),
                  o = i;
            var neg = negDistr[i];
            while (neg == null || neg.Id.Equals(vec.Id)) {
                i = global::Random.Next(negDistr.Length);
                if (i == o) {
                    return null;
                }
                neg = negDistr[i];
            }
            if (vec.Id.Equals(neg.Id)) {
                return null;
            }
            return neg;
        }

        public static double learnWindow(Matrix<Word> Model, Word[] RandDistr, string[] textFragment,
            int iter, Func<bool> HasCtrlBreak, ref int verbOut) {
            Debug.Assert(WINDOW >= 0 && WINDOW <= 13);
            Debug.Assert(NEGATIVES >= 0 && NEGATIVES <= 13);
            Set bow = new Set(),
                    negs = new Set();
            double loss = 0,
                cc = 0;
            Word wo = null;
            int mid = (textFragment.Length >> 1) % textFragment.Length,
                    stop = mid;
            wo = Model[textFragment[mid]];
            while (wo == null) {
                mid = (mid + 1) % textFragment.Length;
                if (mid == stop) {
                    break;
                }
                wo = Model[textFragment[mid]];
            }
            if (wo == null) {
                return 0;
            }
            if (WINDOW > 0) {
                bow.Clear();
                int boundry = global::Random.Next(WINDOW + 1);
                while (boundry == 0) {
                    boundry = global::Random.Next(WINDOW + 1);
                }
                for (var c = mid - boundry; c <= mid + boundry; c++) {
                    if (c >= 0 && c < textFragment.Length && textFragment[c] != null
                                && !wo.Id.Equals(textFragment[c])) {
                        var wi = Model[textFragment[c]];
                        if (wi != null) {
                            bow.Push(textFragment[c]);
                        }
                    }
                }
                if (bow.Count > 0) {
                    const bool POSITIVE = true;
                    loss += sgd(Matrix<Word>.Select(Model, bow, bow.Count),
                        wo,
                        lr,
                        POSITIVE,
                        iter, ref verbOut);
                    cc++;
                }
            }
            if (NEGATIVES > 0) {
                negs.Clear();
                int boundry = global::Random.Next(NEGATIVES + 1);
                while (boundry == 0) {
                    boundry = global::Random.Next(NEGATIVES + 1);
                }
                for (var c = 0; c < boundry; c++) {
                    var neg = PickFromNegDistr(RandDistr, wo);
                    if (neg != null
                            && !wo.Id.Equals(neg.Id)
                            && !bow.Has(neg.Id)) {
                        negs.Push(neg.Id);
                    }
                }
                if (negs.Count > 0) {
                    const bool NEGATIVE = false;
                    loss += sgd(Matrix<Word>.Select(Model, negs, negs.Count),
                        wo,
                        lr,
                        NEGATIVE,
                        iter, ref verbOut);
                    cc++;
                }
            }
            if (cc > 0) {
                loss /= cc;
            }
            return loss;
        }

        static double sgd(Word[] Wi, Word wo,
            double lr, bool label, int gen,
            ref int verbOut) {
            if (Wi == null || Wi.Length <= 0 || wo == null) {
                return 0;
            }
            double loss = 0,
                      cc = 0,
                   score = 0,
                      err = 0;
            double[] input = computeInputVector(Wi);
            double[] grads
                = new double[input.Length];
            score = binaryLogistic(
                input,
                grads,
                wo.Elements,
                label ? 1.0 : 0.0,
                lr);
            if (label) {
                err = -System.Math.Log(score);
            } else {
                err = -System.Math.Log(1.0 - score);
            }
            if (!double.IsNaN(err) && !double.IsInfinity(err)) {
                loss += err;
                cc++;
            } else {
                Console.WriteLine("NaN or Infinity detected...");
            }
            _LOG_(Wi, wo, label, gen,
                ref verbOut, score, err);
            if (cc > 0) {
                loss = loss / cc;
                for (var j = 0; j < grads.Length; j++) {
                    grads[j] /= (double)cc;
                }
                backPropGrads(Wi, grads);
            }
            return loss;
        }

        static void backPropGrads(Word[] Wi, double[] grads) {
            foreach (Word wi in Wi) {
                if (Wi == null) continue;
                Debug.Assert(wi.Elements.Length == grads.Length);
                for (var j = 0; j < wi.Elements.Length; j++) {
                    wi.Elements[j].Re += (float)grads[j];
                }
            }
        }

        static double[] computeInputVector(IEnumerable<Word> Wi) {
            double[] wi = null;
            int cc = 0;
            foreach (Word i in Wi) {
                if (i == null) continue;
                if (wi == null) {
                    wi = new double[i.Elements.Length];
                }
                Debug.Assert(wi.Length == i.Elements.Length);
                for (var j = 0; j < wi.Length; j++) {
                    wi[j] += i.Elements[j].Re;
                }
                cc++;
            }
            if (cc > 0) {
                for (var j = 0; j < wi.Length; j++) {
                    wi[j] /= cc;
                }
            }
            return wi;
        }

        public static double binaryLogistic(double[] wi,
            double[] grads, Complex[] wo, double label, double lr) {
            if (wo == null) return 0;
            Debug.Assert(label >= -1 && label <= 1);
            Debug.Assert(lr >= 0 && lr <= 1);
            int len = wi.Length;
            double dot = 0.0;
            for (int j = 0; j < len; j++) {
                dot += wo[j].Im * wi[j];
            }
            var y = Sigmoid.f(dot);
            double diff = lr * (label - y);
            if (double.IsNaN(diff) || double.IsInfinity(diff)) {
                Console.WriteLine("NaN or Infinity detected...");
                return diff;
            }
            if (grads != null) {
                Debug.Assert(grads.Length == len);
                for (int j = 0; j < len; j++) {
                    grads[j] += wo[j].Im * diff;
                }
            }
            for (int j = 0; j < len; j++) {
                wo[j].Im += (float)(wi[j] * diff);
            }
            return y;
        }

        static void _LOG_(Word[] input, Word output, bool label, int gen,
            ref int verbOut, double score, double err) {
            if (err == 0 || (verbOut >= 0 && ((verbOut % VERBOSITY) == 0))) {
                Console.Write($"[{Thread.CurrentThread.ManagedThreadId}.{verbOut}] " +
                    $"p(y = {Convert.ToInt32(label)}, {output.Id} | {string.Join<Word>(", ", input)})" +
                    $" = {score}\r\n");
            }
            Interlocked.Increment(ref verbOut);
        }
    }

    public static partial class CBOW {
        public static void SaveToFile(Word[] Model, string fmt, int dims, string outputFilePath) {
            SaveToFile(
                Model,
                fmt,
                dims,
                outputFilePath,
                0,
                Model.Length);
        }
        public static void SaveToFile(Word[] Model, string fmt, int dims, string outputFilePath,
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
                    Word it = Model[sample];
                    if (it == null) {
                        continue;
                    }
                    var sb = new StringBuilder();
                    Complex[] axis = it.Elements;
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
                    var score = it.ToString(z: true);
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

        public static Matrix<Word> LoadFromFile(string inputFilePath, int size, out string fmt, out int dims) {
            Matrix<Word> Model = new Matrix<Word>((id, hashCode) => new Word(id, hashCode), size);
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
                    ParseWord(Model, fmt, l, dims);
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
        static void ParseWord(Matrix<Word> Model, string fmt, string sz, int dims) {
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
            Word vec;
            if (fmt == "CBOW" || fmt == "MEL" || fmt == "CLI") {
                vec = Model.Push(w);
                if (vec.Elements == null) {
                    vec.Alloc(dims);
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
                            vec.Re = float.Parse(Re);
                            if (!string.IsNullOrWhiteSpace(Im)) {
                                vec.Im = ImSign * float.Parse(Im);
                            }
                            break;
                        case 1:
                            vec.Elements[n].Re = float.Parse(Re);
                            if (!string.IsNullOrWhiteSpace(Im)) {
                                vec.Elements[n].Im = ImSign * float.Parse(Im);
                            }
                            n++;
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

    }
}