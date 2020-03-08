namespace System.Collections {
    public class Vector : Dot {
        public Complex[] Axis;
        public Vector(string id, float[] re, float[] im)
            : base(id, ComputeHashCode(id)) {
            int len = 0;
            if (re != null) {
                len = re.Length;
            }
            if (im != null) {
                if (re != null) {
                    if (len != im.Length) {
                        throw new ArgumentException();
                    }
                } else {
                    len = im.Length;
                }
            }
            Axis = new Complex[len];
            for (int i = 0; i < len; i++) {
                if (re != null) {
                    Axis[i].Re = re[i];
                }
                if (im != null) {
                    Axis[i].Im = im[i];
                }
            }
        }
        public int Length { get => Axis.Length; }
        public Vector(string id)
            : base(id, ComputeHashCode(id)) {
        }
        public Vector(string id, int hashCode)
            : base(id, hashCode) {
        }
        public void Alloc(int len) {
            if (Axis != null) {
                throw new InvalidOperationException();
            }
            if (len < 0 || len > 1024) {
                throw new ArgumentOutOfRangeException(nameof(len));
            }
            Axis = new Complex[len];
        }
    }
}