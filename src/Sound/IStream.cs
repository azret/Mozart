using System;

public interface IStream {
    float Phase { get; }
    float Hz { get; }
    float[] Peek();
    void Push(float[] X);
}