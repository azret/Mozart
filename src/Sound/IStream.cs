using System;

public interface IStream {
    float Hz { get; }
    float[] Peek();
    void Push(float[] X);
}