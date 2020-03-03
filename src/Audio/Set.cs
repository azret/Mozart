using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Audio {
    [DebuggerDisplay("{Seconds}s")]
    public class Set : IEnumerable<Frequency> {
        public readonly float Seconds;
        readonly
            IEnumerable<Frequency> _items;
        public Set(float seconds, IEnumerable<Frequency> items) {
            Seconds = seconds;
            _items = items;
        }
        public IEnumerator<Frequency> GetEnumerator() {
            if (_items == null) {
                yield break;
            }
            foreach (Frequency f in _items) {
                if (f.Freq > 0) {
                    yield return f;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
    }
}