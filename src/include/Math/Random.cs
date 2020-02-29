public class Random {
    static long Seed = System.Environment.TickCount;
    public static int Next(int max = int.MaxValue) {
        Seed = Seed * 25214903917 + 11;
        int i = ((int)(Seed & 0x7FFFFFFF)) % max;
        return i;
    }
    public static void Shuffle<T>(T[] items, int length) {
        for (int i = 0; i < length; i++) {
            T j = items[i];
            int n = Next(length);
            items[i] = items[n];
            items[n] = j;
        }
    }
}