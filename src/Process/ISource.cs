using System;

public interface ISource {
    int Hz { get; }
    Complex[] Peek();
    void Push(Complex[] fft);
}