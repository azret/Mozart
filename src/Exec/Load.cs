using System;
using System.Ai;
using System.Collections;
using System.IO;
using System.Reflection;

static partial class App {
    static bool Load(Session session,
        string load,
        Func<bool> IsTerminated) {
        if (load.StartsWith("--load")) {
            load = load.Remove(0, "--load".Length).Trim();
        } else if (load.StartsWith("load")) {
            load = load.Remove(0, "load".Length).Trim();
        } else {
            throw new ArgumentException();
        }
        session.ChangeModel(
            Model.LoadFromFile(
                Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".cli"),
                System.Ai.Cli.SIZE,
                out string fmt,
                out int dims));
        return false;
    }

    public static Matrix BuildFromFragment(string sl) {
        Matrix Model = new Matrix(1);
        if (!string.IsNullOrWhiteSpace(sl)) {
            ParseNotes(Model, sl, CBOW.DIMS);
        }
        return Model;
    }

    static void ParseNotes(Matrix Model, string sl, int dims) {
        int i = 0, wordStart = i;
        while (i < sl.Length && (sl[i] == '\t' || sl[i] == ' '
                        || sl[i] == '•' || sl[i] == '|' || sl[i] == '⁞')) {
            i++;
        }
        Vector vec;
        int t = Model.Count;
        if (t >= Model.Capacity) {
            throw new OutOfMemoryException();
        }
        vec = new Vector("𝆕");
        if (vec.Axis == null) {
            vec.Axis = new Complex[dims];
        }
        Model[t] = vec;
        for (; ; ) {
            wordStart = i;
            while (i < sl.Length && (sl[i] == '±' || sl[i] == '-' || sl[i] == '+' || sl[i] == 'E'
                    || sl[i] == 'A' || sl[i] == 'B' || sl[i] == 'C'
                    || sl[i] == 'D' || sl[i] == 'F' || sl[i] == 'G'
                    || sl[i] == '#'
                    || sl[i] == '.' || char.IsDigit(sl[i]))) {
                i++;
            }
            var ImSign = +1;
            int len = (i - wordStart) - 1;
            while (wordStart + len > 0 && wordStart + len < sl.Length
                    && (sl[wordStart + len] == '-' || sl[wordStart + len] == '+' || sl[wordStart + len] == '±')) {
                if (sl[wordStart + len] == '-') {
                    ImSign = -1;
                }
                len--;
            }
            string Re = sl.Substring(wordStart, len + 1);
            string Im = null;
            if (i < sl.Length && (sl[i] == 'i')) {
                i++;
                wordStart = i;
                while (i < sl.Length && (sl[i] == '-' || sl[i] == '+' || sl[i] == 'E'
                        || sl[i] == 'A' || sl[i] == 'B' || sl[i] == 'C'
                        || sl[i] == 'D' || sl[i] == 'F' || sl[i] == 'G'
                        || sl[i] == '#'
                        || sl[i] == '.' || char.IsDigit(sl[i]))) {
                    i++;
                }
                Im = sl.Substring(wordStart, i - wordStart);
            }
            if (!string.IsNullOrWhiteSpace(Re)) {
                var f = Envelopes.NOTE2FREQ(Re);
                int m = Envelopes.FREQ2MIDI(f);
                if (m >= 0 && m < vec.Axis.Length) {
                    vec.Axis[m].Im = f;
                    vec.Axis[m].Re = 1;
                    if (!string.IsNullOrWhiteSpace(Im)) {
                        vec.Axis[m].Re = Envelopes.Amplitude((int)(ImSign * double.Parse(Im)));
                    }
                }
                while (i < sl.Length && (sl[i] == '\t' || sl[i] == ' ' || sl[i] == '•' || sl[i] == '|' || sl[i] == '⁞')) {
                    i++;
                }
            } else /* End of Line */ {
                break;
            }
        }
    }
}