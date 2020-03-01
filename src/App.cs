using System;
using System.Threading;

unsafe partial class App {
    public string CurrentDirectory {
        get => Environment.CurrentDirectory.Trim('\\', '/');
        set => Environment.CurrentDirectory = value;
    }

    readonly Stream Stream = new Stream();

    public App() {
        // bool bStarted = false;
        // _hTimer = new Timer((state) => {
        //     if (!bStarted) return;
        //     Loop();
        //     Notify(null, IntPtr.Zero);
        // }, null, 0, Timeout.Infinite);
        // bStarted = true;
        // _hTimer.Change(100, 100);

        Interlocked.Exchange(
            ref Mic,
            OpenMic())?.Dispose();
    }

    static void Main() {
        runCli(new App());
    }
}