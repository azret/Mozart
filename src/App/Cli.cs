using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.Plot2D;

unsafe partial class App {
    static void runCli(App app) {
        var HasCtrlBreak = false;
        Console.CancelKeyPress += OnCancelKey;
        try {
            Console.InputEncoding
                = Console.OutputEncoding = Encoding.UTF8;
            var cliScript = Environment.CommandLine;
            string exe = Environment.GetCommandLineArgs().First();
            if (cliScript.StartsWith($"\"{exe}\"")) {
                cliScript = cliScript.Remove(0, $"\"{exe}\"".Length);
            }
            else if (cliScript.StartsWith(exe)) {
                cliScript = cliScript.Remove(0, exe.Length);
            }
            cliScript
                = cliScript.Trim();
            if (!string.IsNullOrWhiteSpace(cliScript)) {
                HasCtrlBreak = Exec(
                    app,
                    cliScript,
                    () => HasCtrlBreak);
            }
            while (!HasCtrlBreak) {
                Console.Write($"\r\n{app.CurrentDirectory}>");
                Console.Title = Path.GetFileNameWithoutExtension(
                    typeof(App).Assembly.Location);
                cliScript = Console.ReadLine();
                if (cliScript == null) {
                    HasCtrlBreak = false;
                    continue;
                }
                try {
                    HasCtrlBreak = Exec(
                        app,
                        cliScript,
                        () => HasCtrlBreak);
                } catch(Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
        } finally {
            Console.CancelKeyPress -= OnCancelKey;
        }
        void OnCancelKey(object sender, ConsoleCancelEventArgs e) {
            HasCtrlBreak = true;
                e.Cancel = true;
        }
    }

    static bool Exec(
            App app,
            string cliString,
            Func<bool> IsTerminated) {
        if (cliString.StartsWith("--viz", StringComparison.OrdinalIgnoreCase) || cliString.StartsWith("viz", StringComparison.OrdinalIgnoreCase)) {
            return ExecViz(
                app,
                cliString,
                IsTerminated);
        // } else if (cliString.StartsWith("--mic", StringComparison.OrdinalIgnoreCase) || cliString.StartsWith("mic", StringComparison.OrdinalIgnoreCase)) {
        //     return OpenMicSignalWindow(
        //         app,
        //         cliString,
        //         IsTerminated);
        // } else if (cliString.StartsWith("--fft", StringComparison.OrdinalIgnoreCase) || cliString.StartsWith("fft", StringComparison.OrdinalIgnoreCase)) {
        //     return OpenMicFastFourierTransformWindow(
        //         app,
        //         cliString,
        //         IsTerminated);
        // } else if (cliString.StartsWith("--md", StringComparison.OrdinalIgnoreCase) || cliString.StartsWith("md", StringComparison.OrdinalIgnoreCase)) {
        //     return OpenMicMidiWindow(
        //         app,
        //         cliString,
        //         IsTerminated);
        } else if (cliString.StartsWith("cd", StringComparison.OrdinalIgnoreCase)) {
            var dir = cliString.Remove(0, "cd".Length).Trim();
            if (Directory.Exists(dir)) {
                app.CurrentDirectory = dir;
            }
        } else if (cliString.StartsWith("cls", StringComparison.OrdinalIgnoreCase)) {
            Console.Clear();
        } else {
            return PlayFrequency(
                app,
                cliString,
                IsTerminated);
        }
        return false;
    }
}