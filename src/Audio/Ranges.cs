namespace System.Audio {
    /// <summary>
    /// The cochlea is capable of exceptional sound analysis, in terms of both frequency and intensity.
    ///     The human cochlea allows the perception of sounds between 20 Hz and 20 000 Hz (nearly 10 octaves), 
    ///     with a resolution of 1/230 octave(from 3 Hz at 1000 Hz).
    ///     At 1000 Hz, the cochlea encodes acoustic pressures between 0 dB SPL (2 x 10-5 Pa) and 120 dB SPL (20 Pa).
    /// </summary>
    public static class Ranges {
        /// <summary>
        /// Piano (88 keys): 27.5 Hz (A0) to 4186.3 Hz (C8).
        /// </summary>
        public static double[] Freqs = { 27.5, 4186.3 };
        /// <summary>
        /// Checks if a given sample is in valid sound range.
        /// </summary>
        public static bool IsInRange(double freq, int dB) {
            bool bInRange = false;
            if ((freq >= Ranges.Freqs[0] && freq <= Ranges.Freqs[1])) {
                bInRange = true;
            }
            if (bInRange) {
                bInRange = false;
                if (dB >= -60 && dB <= 60) {
                    bInRange = true;
                }
            }
            return bInRange;
        }
    }
}