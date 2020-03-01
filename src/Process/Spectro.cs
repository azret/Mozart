using System;

public class Spectro {
    public readonly int Hz = 44100;

    object _lock = new object();

    Complex[] _last;

    public void Push(Complex[] fft) {
        lock (_lock) {
            var last = fft != null
                ? (Complex[])fft.Clone()
                : null;
            _last = last;
        }
    }

    public Complex[] Peek() {
        lock (_lock) {
            var last = _last != null
                ? (Complex[])_last.Clone()
                : null;
            return last;
        }
    }
}