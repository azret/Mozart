using System.Collections.Generic;
using System.Threading;

namespace System.Collections {
    public partial class Matrix<T> : IEnumerable<T>
            where T : Dot {
        protected Func<string, int, T> _factory;
        public Matrix(Func<string, int, T> factory, int length) {
            if (length > 31048576 || length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _data = new T[length];
        }
        public Matrix(Func<string, int, T> factory, T[] hash, int count) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _data = hash ?? throw new ArgumentNullException(nameof(hash));
            _count = count;
        }
        protected int _count;
        public int Count => _count;
        protected T[] _data;
        public int Capacity { get => _data.Length; }
        public T this[string id, int hashCode] {
            get {
                int index = Dot.LinearProbe(_data, id, hashCode,
                    out T row, out int depth);
                if (index < 0) {
                    return null;
                }
                return row;
            }
        }
        public T this[string id] {
            get {
                if (id == null || id.Length == 0) return /*!*/ null;
                int index = Dot.LinearProbe(_data, id, Dot.ComputeHashCode(id),
                    out T row, out int depth);
                if (index < 0) {
                    return /*!*/ null;
                }
                return row;
            }
        }
        protected int _depth;
        public int Depth { get => _depth; }
        public T Push(string key) { return Push(key, Dot.ComputeHashCode(key)); }
        public T Push(string key, int hashCode) {
            for (; ;) {
                int index = Dot.LinearProbe(_data, key, hashCode,
                    out T row, out int depth);
                if (index < 0) {
                    throw new OutOfMemoryException();
                }
                if (row != null) {
                    Diagnostics.Debug.Assert(row.Id.Equals(key));
                    return row;
                } else {
                    T pre = Interlocked.CompareExchange(ref _data[index],
                                    row = _factory(key, hashCode), null);
                    if (pre != null) {
                        continue;
                    }
                    if (depth > _depth) {
                        _depth = depth;
                    }
                    Interlocked.Increment(ref _count);
                    return row;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() {
            int count = 0;
            for (int i = 0; i < _data.Length; i++) {
                var row = _data[i];
                if (row != null) {
                    count++;
                    yield return row;
                }
            }
            if (count != _count) {
                throw new InvalidOperationException();
            }
        }
    }

    public partial class Matrix<T> {
        public static T[] Sequence(Matrix<T> M) {
            T[] list = new T[M.Count]; int n = 0;
            for (int i = 0; i < M._data.Length; i++) {
                T row = M._data[i];
                if (row != null) {
                    list[n++] = row;
                }
            }
            if (n != M._count) {
                throw new InvalidOperationException();
            }
            if (n < M._count) {
                Array.Resize(ref list, n);
            }
            return list;
        }
        public static T[] Select(Matrix<T> M, IEnumerable<string> items, int max) {
            if (M == null) {
                return null;
            }
            T[] list = new T[max]; int n = 0;
            foreach (var q in items) {
                T row = M[q];
                if (row != null) {
                    if (n + 1 > max) {
                        break;
                    }
                    list[n++] = row;
                }
            }
            if (n != list.Length) {
                Array.Resize(ref list, n);
            }
            return list;
        }
        public static T[] Sort(Matrix<T> M, int skip = 0, int take = int.MaxValue) {
            T[] sort = Sequence(M);
            Array.Sort(
                sort,
                (a, b) => -a.Re.CompareTo(b.Re));
            if (take < sort.Length) {
                Array.Resize(ref sort, take);
            }
            return sort;
        }
        public static T[] Sort(Matrix<T> M, Comparison<T> comparison) {
            T[] sort = Sequence(M);
            Array.Sort(
                sort,
                comparison);
            return sort;
        }
    }

    // public partial class Matrix : Matrix<Vector> {
    //     public Matrix(int length) 
    //         : base((id, hashCode) => new Vector(id, hashCode), length) {
    //     }
    //     public Matrix(Vector[] data, int count) 
    //         : base((id, hashCode) => new Vector(id, hashCode), data, count) {
    //     }
    // }
}