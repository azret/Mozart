using System;

public class Stream {
    public readonly int Samples = 1024;
    public readonly int Hz = 44100;

    object _lock = new object();

    Complex[] _peek;

    public Complex[][] _buffer = new Complex[1024][];

    public void Push(Complex[] fft) {
        lock (_lock) {
            var last = fft != null
                ? (Complex[])fft.Clone()
                : null;
            _peek = last;
            for (int i = 0; i < _buffer.Length - 1; i++) {
                _buffer[i] = _buffer[i + 1];
            }
            _buffer[_buffer.Length - 1] = last;
        }
    }

    public Complex[] Peek() {
        lock (_lock) {
            var last = _peek != null
                ? (Complex[])_peek.Clone()
                : null;
            return last;
        }
    }
}