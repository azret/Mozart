using System;
using System.Threading;

public class Source : ISource {
    object _lock = new object();

    Complex[] _peek;

    public int Hz => 44100;

    // int _cc;
    // Complex[][] _buffer = new Complex[1024][];

    public void Push(Complex[] fft) {
        lock (_lock) {
            var last = fft != null
                ? (Complex[])fft.Clone()
                : null;
            _peek = last;
            // for (int i = 0; i < _buffer.Length - 1; i++) {
            //     _buffer[i] = _buffer[i + 1];
            // }
            // _buffer[_buffer.Length - 1] = last;
            // _cc++;
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

    // public int Peek(out Complex[][] buffer) {
    //     lock (_lock) {
    //         if (_cc < _buffer.Length) {
    //             buffer = new Complex[_cc][];
    //             for (int i = 0; i < _cc; i++) {
    //                 buffer[buffer.Length - i - 1] = _buffer[_buffer.Length - i - 1];
    //             }
    //             return _cc;
    //         } else {
    //             buffer = (Complex[][])_buffer.Clone();
    //             return buffer.Length;
    //         }
    //     }
    // }
}