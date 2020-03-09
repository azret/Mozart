namespace System.Collections {
    public class Word : Vector {
        public Word(string id) : base(id) {}
        public Word(string id, int hashCode) 
            : base(id, hashCode) {}
        public Word(string id, float[] re, float[] im)
            : base(id, re, im) {}
    }
}