using System;
using System.Ai;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

static partial class App {
    static bool Train(Session session,
            string cliScript,
            Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--train")) {
            cliScript = cliScript.Remove(0, "--train".Length).Trim();
        } else if (cliScript.StartsWith("train")) {
            cliScript = cliScript.Remove(0, "train".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        System.Ai.Cli.Train(session.Model, 37,
            (loss)=> { }, IsTerminated);
        Model.SaveToFile(
            session.Model.GetBuffer(),
            "CLI",
            37,
            Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".cli"));
        return false;
    }

    const int SIZE = 1048576,
        GENS = (int)1e6,
            SHUFFLE = (int)1e7;

    static bool ScoreMelModel(Session session,
        string cliScript,
        Func<bool> IsTerminated) {
        if (cliScript.StartsWith("--score")) {
            cliScript = cliScript.Remove(0, "--score".Length).Trim();
        } else if (cliScript.StartsWith("score")) {
            cliScript = cliScript.Remove(0, "score".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        ScoreMelModel(MakeFiles(new string[] { session.CurrentDirectory },
                    ".md", SearchOption.TopDirectoryOnly),
            session.Model);
        return false;
    }

    static bool TrainMelModel(Session session,
        string train,
        Func<bool> IsTerminated) {
        if (train.StartsWith("--train")) {
            train = train.Remove(0, "--train".Length).Trim();
        } else if (train.StartsWith("train")) {
            train = train.Remove(0, "train".Length).Trim();
        } else {
            throw new ArgumentException();
        }

        TrainMelModel(MakeFiles(new string[] { Path.Combine(session.CurrentDirectory, "source") },
                    ".md", SearchOption.TopDirectoryOnly),
            session.Model,
            (loss) => { },
            IsTerminated);
        ScoreMelModel(MakeFiles(new string[] { session.CurrentDirectory },
                    ".md", SearchOption.TopDirectoryOnly),
            session.Model);

        Model.SaveToFile(
            session.Model.GetBuffer(),
            "MEL",
            CBOW.DIMS,
            session.OutputFileName);

        return false;
    }

    static bool TrainMikolovModel(Session session,
        string train,
        Func<bool> IsTerminated) {
        if (train.StartsWith("--train")) {
            train = train.Remove(0, "--train".Length).Trim();
        } else if (train.StartsWith("train")) {
            train = train.Remove(0, "train".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        TrainMikolovModel(MakeFiles(new string[] { session.CurrentDirectory },
                    session.SearchPattern, SearchOption.AllDirectories),
            session.Lex,
            session.Model,
            (loss) => { },
            IsTerminated);
        Model.SaveToFile(
            Matrix.Sort(session.Model),
            "CBOW",
            CBOW.DIMS,
            session.OutputFileName);
        return false;
    }

    static void TrainMelModel(Set sourceFiles,
        Matrix Model, Action<double> SetLoss, Func<bool> HasCtrlBreak) {
        if (Model == null) {
            Console.WriteLine("Model not loaded.");
            return;
        }
        Thread[] threads = new Thread[Environment.ProcessorCount * 2];
        int numberOfThreads = 0,
            verbOut = 0;
        for (var t = 0; t < threads.Length; t++) {
            threads[t] = new Thread(() => {
                string[] Shuffle = ((IEnumerable<string>)sourceFiles).ToArray();
                Random.Shuffle(
                    Shuffle,
                    Shuffle.Length);
                Interlocked.Increment(ref numberOfThreads);
                try {
                    for (int iter = 0; iter < GENS; iter++) {
                        if (HasCtrlBreak != null && HasCtrlBreak()) {
                            break;
                        }
                        foreach (string file in Shuffle) {
                            if (HasCtrlBreak != null && HasCtrlBreak()) {
                                return;
                            }
                            try {
                                Matrix Data = null;
                                Data = System.Ai.Model.LoadFromFile(file, SIZE, out string fmt, out CBOW.DIMS);
                                Debug.Assert(fmt == "MIDI");
                                double loss = 0,
                                      cc = 0;
                                var wo = Model["a"];
                                if (wo == null) {
                                    Interlocked.Increment(ref verbOut);
                                    continue;
                                }
                                foreach (var it in Data) {
                                    double   score = 0,
                                          err = 0;
                                    double[] X
                                        = new double[it.Axis.Length];
                                    for (int j = 0; j < X.Length; j++) {
                                        X[j] = it.Axis[j].Re;
                                    }
                                    bool label = it.Id == wo.Id;
                                    if (!label) {
                                        for (int j = 0; j < X.Length; j++) {
                                            X[j] = ((Random.Next() & 0xFFFF) / (65536f));
                                            if (X[j] < 0 || X[j] > 0.7) {
                                                X[j] = 0;
                                            }
                                        }
                                        if (((Random.Next() & 0xFFFF) / (65536f)) > 0.7) {
                                            for (int j = 0; j < X.Length; j++) {
                                                X[j] = 0;
                                            }
                                        }
                                    }
                                    score = CBOW.binaryLogistic(
                                        X,
                                        null,
                                        wo.Axis,
                                        label ? 1.0 : 0.0,
                                        CBOW.lr);
                                    if (label) {
                                        err = -System.Math.Log(score);
                                    } else {
                                        err = -System.Math.Log(1.0 - score);
                                    }
                                    if (double.IsNaN(err) || double.IsInfinity(err)) {
                                        Console.WriteLine("NaN detected...");
                                    } else {
                                        loss += err;
                                        cc++;
                                        if (err == 0 || (verbOut >= 0 && ((verbOut % CBOW.VERBOSITY) == 0))) {
                                            Console.Write($"[{Thread.CurrentThread.ManagedThreadId}.{verbOut}] " +
                                                $" Cost: {loss / cc}\r\n");
                                        }
                                        Interlocked.Increment(ref verbOut);
                                    }
                                }
                                Thread.Sleep(3000 + Random.Next(3000));
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

    static void ScoreMelModel(Set sourceFiles, Matrix Model) {
        if (Model == null) {
            Console.WriteLine("Model not loaded.");
            return;
        }
        string[] Shuffle = ((IEnumerable<string>)sourceFiles).ToArray();
        foreach (string file in Shuffle) {
            Matrix Data = System.Ai.Model.LoadFromFile(file, SIZE, out string fmt, out CBOW.DIMS);
            Debug.Assert(fmt == "MIDI");
            var wo = Model["a"];
            if (wo == null) {
                continue;
            }
            foreach (var it in Data) {
                double dot = 0.0,
                    score;
                for (int j = 0; j < it.Axis.Length; j++) {
                    dot += it.Axis[j].Re * wo.Axis[j].Im;
                }
                score = Sigmoid.f(dot);
                it.Score.Im = score;
                it.Axis = null;
            }
            SaveMidi(Data.GetBuffer(), fmt, Path.ChangeExtension(file, ".score"));
        }
    }

    static void TrainMikolovModel(Set sourceFiles, IOrthography lex,
        Matrix Model, Action<double> SetLoss, Func<bool> HasCtrlBreak) {
        if (Model == null) {
            Console.WriteLine("Model not loaded.");
            return;
        }
        Vector[] negDistr =  System.Ai.CBOW.CreateNegDistr(
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
                        Random.Shuffle(Shuffle, Shuffle.Length);
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
                                Thread.Sleep(3000 + Random.Next(3000));
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
}