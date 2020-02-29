namespace System {
    public partial class Scalar : IEquatable<Scalar>, IComparable<Scalar> {
        public static int ComputeHashCode(string id) {
            uint h = 2166136261;
            unchecked {
                h = h ^ (uint)id.GetHashCode();
                h = h * 16777619;
                return (int)h & 0x7FFFFFFF;
            }
        }
        public static int LinearProbe<T>(T[] hash, string id, int hashCode,
            out T value, out int depth) where T : Scalar {
            value = null; depth = 0;
            if (hash == null) {
                return -1;
            }
            var cc = hash.Length;
            int i = hashCode % cc,
                         start = i;
            depth = 0;
            value = hash[i];
            while (value != null && (!(value.GetHashCode() == hashCode && value.Equals(id)))) {
                i = (i + 1) % cc;
                depth++;
                if (i == start) {
                    return -1;
                }
                value = hash[i];
            }
            return i;
        }
        public Scalar() {
            HashCode = base.GetHashCode();
        }
        public Scalar(string id, int hashCode) {
            if (id == null || id.Length == 0) {
                throw new ArgumentNullException(nameof(id));
            }
            if (hashCode < 0) {
                throw new ArgumentOutOfRangeException(nameof(hashCode));
            }
            Id = id;
            HashCode = hashCode;
        }
        public readonly string Id;
        public Complex Score;
        public double Add(double re = 1.0) {
            Score.Re = Score.Re + re;
            return Score.Re;
        }
        public double Multi(double re = 1.0) {
            Score.Re = Score.Re * re;
            return Score.Re;
        }
        public readonly int HashCode;
        public override int GetHashCode() => HashCode;
        public override string ToString() { return Id; }
        public override bool Equals(object other) {
            if (other == null) { return this == null; }
            if (ReferenceEquals(other, this)) { return true; }
            if (other is string s) { return string.Equals(Id, s); }
            if (other is Scalar g) { return Equals(g); }
            return false;
        }
        public bool Equals(Scalar other) {
            if (other == null) { return this == null; }
            if (ReferenceEquals(other, this)) { return true; }
            return string.Equals(Id, other.Id);
        }
        public static int CompareTo(Scalar a, Scalar b) {
            if (a == null) {
                return b == null
                    ? 0
                    : -1;
            } else if (b == null) {
                return a == null
                    ? 0
                    : 1;
            } else {
                return a.Score.Re.CompareTo(b.Score.Re);
            }
        }
        public int CompareTo(Scalar other) {
            return CompareTo(this, other);
        }
    }
}