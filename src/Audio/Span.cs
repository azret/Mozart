using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Audio {
    [DebuggerDisplay("{Seconds}s")]
    public class Span : IEnumerable<Frequency> {
        public readonly float Seconds;
        readonly
            IEnumerable<Frequency> _keys;
        public Span(float seconds, IEnumerable<Frequency> keys) {
            Seconds = seconds;
            _keys = keys;
        }
        public IEnumerator<Frequency> GetEnumerator() {
            if (_keys == null) {
                yield break;
            }
            foreach (Frequency f in _keys) {
                if (f.Freq > 0) {
                    yield return f;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return _keys.GetEnumerator();
        }
    }
}