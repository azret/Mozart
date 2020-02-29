using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Ai {
    public static partial class Cli {
        public const int GENS = (int)1e6,
            SIZE = 48576;

        public static Matrix Create(int dims) {
            var Model = new Matrix(SIZE);
            var data = Generate(dims);
            foreach (var it in data) {
                Model.Push(it.Item1);
            }
            foreach (var it in Model) {
                it.Alloc(dims);
                for (int m = 0; m < dims; m++) {
                    it.Axis[m].Im = ((global::Random.Next() & 0xFFFF) / (65536f) - 0.5f);
                }
            }
            return Model;
        }

        static Tuple<string, string, double[]>[] Generate(int dims) {
            List<Tuple<string, string, double[]>> data = new List<Tuple<string, string, double[]>>();
            data.AddRange(_APPEND_SAMPLES(FILES, "#file"));
            data.AddRange(_APPEND_SAMPLES(TONES, "#tone"));
            data.AddRange(_APPEND_SAMPLES(MSDOS, "#cli"));
            List<Tuple<string, string, double[]>> _APPEND_SAMPLES(string samples, string id) {
                List<Tuple<string, string, double[]>> local = new List<Tuple<string, string, double[]>>();
                foreach (string cliString in samples.Split('\r', '\n')) {
                    if (string.IsNullOrWhiteSpace(cliString)) {
                        continue;
                    }
                    double[] Re = new double[dims];
                    for (var j = 0; j < Re.Length; j++) {
                        if (j >= cliString.Length) {
                            break;
                        }
                        Re[j] = char.ToUpper(cliString[j]) / (double)128;
                    }
                    data.Add(new Tuple<string, string, double[]>(
                        id, cliString, Re));
                }
                return local;
            }
            return data.ToArray();
        }

        public static void Predict(Matrix Model, int dims, string cliString) {
            var Re = new double[dims];
            for (var j = 0; j < Re.Length; j++) {
                if (j >= cliString.Length) {
                    break;
                }
                Re[j] = char.ToUpper(cliString[j]) / (double)128;
            }
            Console.WriteLine();
            foreach (Vector it in Model) {
                int len = it.Axis.Length;
                double dot = 0.0,
                    y;
                for (int j = 0; j < len; j++) {
                    dot += Re[j] * it.Axis[j].Im;
                }
                y = SigF.f(dot);
                Console.WriteLine($"p({it.Id}|{cliString}) ~ {y}");
            }
        }

        public static void Train(Matrix Model, int dims, Action<double> SetLoss, Func<bool> HasCtrlBreak) {
            Thread[] threads = new Thread[Environment.ProcessorCount * 2];
            int numberOfThreads = 0,
                verbOut = 0;
            double loss = 0,
                      cc = 0;
            for (var t = 0; t < threads.Length; t++) {
                threads[t] = new Thread(() => {
                    Interlocked.Increment(ref numberOfThreads);
                    try {
                        for (int iter = 0; iter < GENS; iter++) {
                            if (HasCtrlBreak != null && HasCtrlBreak()) {
                                break;
                            }
                            var data = Generate(dims);
                            global::Random.Shuffle(
                                data,
                                data.Length);
                            foreach (var it in data) {
                                if (HasCtrlBreak != null && HasCtrlBreak()) {
                                    return;
                                }
                                var Y = Model[it.Item1];
                                if (Y == null) {
                                    Console.WriteLine($"Classifier {it.Item1} not found.");
                                    Interlocked.Increment(ref verbOut);
                                    continue;
                                }
                                sgd(
                                    it.Item2,
                                    it.Item3,
                                    Y,
                                    true,
                                    iter,
                                    ref verbOut,
                                    ref loss,
                                    ref cc);
                                var neg = data[global::Random.Next(data.Length)];
                                if (neg != null && neg.Item1 != it.Item1) {
                                    sgd(
                                        neg.Item2,
                                        neg.Item3,
                                        Y,
                                        false,
                                        iter,
                                        ref verbOut,
                                        ref loss,
                                        ref cc);
                                }
                                Thread.Sleep(500 + global::Random.Next(1000));
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

        static void sgd(string x, double[] X, Vector Y, bool label, int iter,
            ref int verbOut, ref double loss, ref double cc) {
            double score = 0, err = 0;
            score = binaryLogistic(
                X,
                null,
                Y.Axis,
                label ? +1.0 : 0.0,
                CBOW.lr);
            if (label) {
                err = -System.Math.Log(score);
            } else {
                err = -System.Math.Log(1.0 - score);
            }
            if (double.IsNaN(err) || double.IsInfinity(err) || double.IsNaN(score) || double.IsInfinity(score)) {
                Console.WriteLine("NaN detected...");
            } else {
                loss += err;
                cc++;
                if (err == 0 || (verbOut >= 0 && ((verbOut % CBOW.VERBOSITY) == 0))) {
                    Console.Write($"[{Thread.CurrentThread.ManagedThreadId}.{iter}.{verbOut}] " +
                        $" Cost: {loss / cc}, p(y={(label ? 1 : 0)}, {Y.Id} | {x}) ~ {Math.Round(score, 2)}\r\n");
                }
                Interlocked.Increment(ref verbOut);
            }
        }

        static double binaryLogistic(double[] X, double[] grads, Complex[] Y, double label, double lr) {
            if (Y == null) return 0;
            Debug.Assert(label >= -1 && label <= 1);
            Debug.Assert(lr >= 0 && lr <= 1);
            int len = X.Length;
            double dot = 0.0;
            for (int j = 0; j < len; j++) {
                dot += Y[j].Im * X[j];
            }
            var y = SigF.f(dot);
            double diff = lr * (label - y);
            if (double.IsNaN(diff) || double.IsInfinity(diff)) {
                Console.WriteLine("NaN detected...");
                return diff;
            }
            if (grads != null) {
                Debug.Assert(grads.Length == len);
                for (int j = 0; j < len; j++) {
                    grads[j] += Y[j].Im * diff;
                }
            }
            for (int j = 0; j < len; j++) {
                Y[j].Im += X[j] * diff;
            }
            return y;
        }

        const string FILES = @"
F:\logs\
C:\data
C:\Program Files\
data.wav
*.*
http://
www.domain.com
ftp://
data.md
data.okurrr
data.txt
.txt
D:\en\
C:\Windows\System32\note.txt
D:\Logs\*.log";

        const string TONES = @"
G9
F#9
F9
E9
D#9
D9
C#9
C9
B8
A#8
A8
G#8
G8
F#8
F8
E8
D#8
D8
C#8
C8
B7
A#7
A7
G#7
G7
F#7
F7
E7
D#7
D7
C#7
C7
B6
A#6
A6
G#6
G6
F#6
F6
E6
D#6
D6
C#6
C6
B5
A#5
A5
G#5
G5
F#5
F5
E5
D#5
D5
C#5
C5
B4
A#4
A4
G#4
G4
F#4
F4
E4
D#4
D4
C#4
C4
B3
A#3
A3
G#3
G3
F#3
F3
E3
D#3
D3
C#3
C3
B2
A#2
A2
G#2
G2
F#2
F2
E2
D#2
D2
C#2
C2
B1
A#1
A1
G#1
G1
F#1
F1
E1
D#1
D1
C#1
C1
B0
A#0
A0
G9
F#9
F9
E9
D#9
D9
C#9
C9
B8
A#8
A8
G#8
G8
F#8
F8
E8
D#8
D8
C#8
C8
B7
A#7
A7
G#7
G7
F#7
F7
E7
D#7
D7
C#7
C7
B6
A#6
A6
G#6
G6
F#6
F6
E6
D#6
D6
C#6
C6
B5
A#5
A5
G#5
G5
F#5
F5
E5
D#5
D5
C#5
C5
B4
A#4
A4
G#4
G4
F#4
F4
E4
D#4
D4
C#4
C4
B3
A#3
A3
G#3
G3
F#3
F3
E3
D#3
D3
C#3
C3
B2
A#2
A2
G#2
G2
F#2
F2
E2
D#2
D2
C#2
C2
B1
A#1
A1
G#1
G1
F#1
F1
E1
D#1
D1
C#1
C1
B0
A#0
A0
G9
G9
F#9
F9
E9
D#9
D9
C#9
C9
B8
A#8
A8
G#8
G8
F#8
F8
E8
D#8
D8
C#8
C8
B7
A#7
A7
G#7
G7
F#7
F7
E7
D#7
D7
C#7
C7
B6
A#6
A6
G#6
G6
F#6
F6
E6
D#6
D6
C#6
C6
B5
A#5
A5
G#5
G5
F#5
F5
E5
D#5
D5
C#5
C5
B4
A#4
A4
G#4
G4
F#4
F4
E4
D#4
D4
C#4
C4
B3
A#3
A3
G#3
G3
F#3
F3
E3
D#3
D3
C#3
C3
B2
A#2
A2
G#2
G2
F#2
F2
E2
D#2
D2
C#2
C2
B1
A#1
A1
G#1
G1
F#1
F1
E1
D#1
D1
C#1
C1
B0
A#0
A0
F#9
F9
E9
D#9
D9
C#9
C9
B8
A#8
A8
G#8
G8
F#8
F8
E8
D#8
D8
C#8
C8
B7
A#7
A7
G#7
G7
F#7
F7
E7
D#7
D7
C#7
C7
B6
A#6
A6
G#6
G6
F#6
F6
E6
D#6
D6
C#6
C6
B5
A#5
A5
G#5
G5
F#5
F5
E5
D#5
D5
C#5
C5
B4
A#4
A4
G#4
G4
F#4
F4
E4
D#4
D4
C#4
C4
B3
A#3
A3
G#3
G3
F#3
F3
E3
D#3
D3
C#3
C3
B2
A#2
A2
G#2
G2
F#2
F2
E2
D#2
D2
C#2
C2
B1
A#1
A1
G#1
G1
F#1
F1
E1
D#1
D1
C#1
C1
B0
A#0
A0
C4 A4 C5
G#3-i3 D#4-i7 F#4-i16 A4-i12
C3-i4 E3-i7 A3-i4 B3-i9 C4-i15 D4-i11
F3-i4 G3-i8 A#3-i11 C4-i18 F4-i4 G4-i4
A#3-i1 C4-i14 C#4-i16 C5-i5 F#5-i2
C3-i1 C#3-i18 F3-i18
D#3-i14 G3-i15dB C#4-i19
D3-i15 B3-i15 A4-i9 F5-i5 D#6-i8 F6-i3
C3-i14 C#3-i12 F#3-i11 G#3-i12 A3+i0 F#4-i6
G3-7 A3+ A3+ C4-6 C4 D4-13
F3-18 G3 A3+4 A3+3dB
G3-5dB A3+3 A3+1 C4-14
G3-2 A3+5 A3+1 C4-13
F3 G3-2 A3+5 A3+2 C4 C4-7
 A3+2 C4-9 C4 D4-14 E4-9
D3 F3 G3-6 A3+4 A3+1 D4-18
E4-9 F4-7 A4-5 A4-20 B4 A5-14 E6-5
C4 D4-20 E4 F4 A4-6 A4 A5 E6-5
C4 E4-7 F4-7 G4 A4-6 A4 C6 E6-6";

        const string MSDOS = @"
assoc 
attrib
break
bcdedit
cacls
call
cd
chcp
chdir
chkdsk
chkntfs
cls
cmd
color
comp
compact
convert
copy
date
del
dir
diskpart
doskey
driverquery
echo
endlocal
erase
exit
fc
find
findstr
for
format
fsutil
ftype
goto
gpresult
graftabl
help
icacls
if
label
md
mkdir
mklink
mode
more
move
openfiles
path
pause
popd
print
prompt
pushd
rd
recover
rem
ren
rename
replace
rmdir
robocopy
set
setlocal
sc
schtasks
shift
shutdown
sort
start
subst
systeminfo
tasklist
taskkill
time
title
tree
type
ver
verify
vol
xcopy
wmic";
    }
}