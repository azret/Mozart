using System;
using System.Ai;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

static partial class App {
    static bool Build(Session session,
            string build,
            Func<bool> IsTerminated) {
        if (build.StartsWith("--build")) {
            build = build.Remove(0, "--build".Length).Trim();
        } else if (build.StartsWith("build")) {
            build = build.Remove(0, "build".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        session.ChangeModel(
            System.Ai.Cli.Create(37));
        Model.SaveToFile(
            session.Model.GetBuffer(),
            "CLI",
            37,
            Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".cli"));
        return false;

        /*
        session.ChangeModel(
            BuildFromPlainText(
                session.CurrentDirectory,
                session.SearchPattern,
                session.Lex,
                session.OutputFileName));
        SaveModel(
            Matrix.Sort(session.Model),
            "CBOW",
            Path.ChangeExtension(session.OutputFileName, ".okurrr"));
        */

        /*
        session.ChangeModel(
            System.Ai.Mel.CreateModel(SIZE));
        SaveModel(
            session.Model.GetBuffer(),
            "MEL",
            Path.ChangeExtension(session.OutputFileName, ".okurrr"));
            */
    }

    static Matrix BuildFromPlainText(string sourcePath, string searchPattern, IOrthography lex, string outputFileName) {
        var Model = new Matrix(SIZE);

        Set SourceFiles = null,
            SkipList = null;

        Matrix AllowList = null;

        var file = Path.ChangeExtension(outputFileName, ".allow");
        if (File.Exists(file)) {
            AllowList = MakeWhiteList(file, lex, SIZE * 3);
        }

        file = Path.ChangeExtension(outputFileName, ".ignore");
        if (File.Exists(file)) {
            SkipList = MakeStops(13452, File.ReadAllText(file), lex);
        }

        ParsePlainTextFiles(
            Model,
            SourceFiles = MakeFiles(new string[] { sourcePath },
                searchPattern, SearchOption.AllDirectories),
            lex,
            SkipList);

        if (AllowList.Count > 0 || CBOW.THRESHOLD > 0) {
            LimitToThreshold(AllowList, ref Model);
        }

        InitializeScores(Model);

        LearnSpellingVariations(
            Model,
            SourceFiles,
            lex);

        return Model;
    }

    static void ParsePlainTextFiles(Matrix Model, Set files, IOrthography lex, Set skipList) {
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
                        it.Add(1d / CBOW.THRESHOLD);
                    }
                }
            }
        }
    }

    static void LimitToThreshold(Matrix Allow, ref Matrix Model) {
        var Dup = new Matrix(SIZE);
        foreach (var h in Model) {
            if (h.Score.Re < 1) {
                continue;
            }
            if (Allow != null) {
                if (Allow[h.Id] == null) {
                    continue;
                }
            }
            Vector l = Dup.Push(
                h.Id,
                h.HashCode);
            // l.Assign(h);
            l.Score = h.Score;
        }
        Model = Dup;
    }

    static void InitializeScores(Matrix Model) {
        double norm = 0;

        foreach (var w in Model) {
            w.Score.Im = /* Just to break the mosaic pattern of the random number generator... */ 
                ((Random.Next() & 0xFFFF) / (65536f) - 0.5f);
            w.Alloc(System.Ai.CBOW.DIMS);
            for (int i = 0; i < w.Axis.Length; i++) {
                w.Axis[i].Re = ((Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                w.Axis[i].Im = ((Random.Next() & 0xFFFF) / (65536f) - 0.5f);
            }
            w.Score.Im = 0;
            if (w.Score.Re > 0) {
                w.Score.Re = Math.Round(w.Score.Re);
            } else {
                w.Score.Re = 0;
            }
            norm = Math.Max(norm,
                w.Score.Re);
        }

        if (norm != 0) {
            foreach (var w in Model) {
                w.Score.Im = w.Score.Re / norm;
            }
        }
    }

    static Set MakeStops(int hashSize, string textFragment, IOrthography lex) {
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

    static Matrix MakeWhiteList(string file, IOrthography lex, int hashSize) {
        var W = new Matrix(hashSize);
        Console.Write($"\r\nReading {file}...\r\n\r\n");
        string textFragment = File.ReadAllText(file);
        foreach (var t in PlainText.ForEach(textFragment, 0, textFragment.Length, 0)) {
            if (t.Type == PlainTextTag.TAG) {
                W.Push(lex.GetKey(t.TextFragment.Substring(t.StartIndex, t.Length)));
            }
        }
        return W;
    }

    static Set MakeFiles(string[] paths, string searchPattern, SearchOption searchOption) {
        Set files = new Set();
        foreach (var path in paths) {
            FileAttributes attr = FileAttributes.Offline;
            try {
                attr = File.GetAttributes(path);
            } catch (System.IO.FileNotFoundException) {
            }
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                foreach (var s in Directory.EnumerateFiles(path, "*.*", searchOption)) {
                    string file = Path.GetFullPath(s).ToLowerInvariant();
                    if (files.Contains(file)) {
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
                        files.Push(file);
                    }
                }
            } else {
                string file = Path.GetFullPath(path).ToLowerInvariant();
                attr = File.GetAttributes(file);
                if ((attr & FileAttributes.Hidden) != FileAttributes.Hidden) {
                    files.Push(file);
                }
            }
        }
        return files;
    }
}