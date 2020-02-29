using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Audio {
    /// <summary>
    /// A chord, in music, is any harmonic set of pitches consisting of multiple notes that 
    ///     are heard as if sounding simultaneously. For many practical and theoretical purposes,
    ///     arpeggios and broken chords, or sequences of chord tones, may also be considered as chords.
    /// </summary>
    /// <see cref="https://en.wikipedia.org/wiki/Chord_(music)"/>
    [DebuggerDisplay("{Seconds}s")]
    public struct Chord : IEnumerable<Frequency> {
        public float Seconds;
        public Frequency[] Frequency;
        public IEnumerator<Frequency> GetEnumerator() {
            if (Frequency != null) {
            }
            foreach (Frequency f in Frequency) {
                if (f.Freq > 0) {
                    yield return f;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return Frequency.GetEnumerator();
        }
    }
}