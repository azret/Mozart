public class Stream : IStream {
    object _lock = new object();

    public float Hz => 44100;

    float[] _peek;

    public void Push(float[] X) {
        lock (_lock) {
            var last = X != null
                ? (float[])X.Clone()
                : null;
            _peek = last;
        }
    }

    public float[] Peek() {
        lock (_lock) {
            var last = _peek != null
                ? (float[])_peek.Clone()
                : null;
            return last;
        }
    }
}