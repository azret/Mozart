﻿/// <summary>
/// ƒ(a) = 1 / (1 + e⁻ᵃ)
/// </summary>
public class SigQ : IFunc {
    public const int SIGMOID = 4;
    public const int _TABLE_SIZE = 512;
    public static double[] _TABLE = {
        0.0180, 0.0183, 0.0185, 0.0188, 0.0191, 0.0194, 0.0197, 0.0200,
        0.0203, 0.0206, 0.0210, 0.0213, 0.0216, 0.0219, 0.0223, 0.0226,
        0.0230, 0.0233, 0.0237, 0.0241, 0.0244, 0.0248, 0.0252, 0.0256,
        0.0260, 0.0264, 0.0268, 0.0272, 0.0276, 0.0280, 0.0284, 0.0289,
        0.0293, 0.0298, 0.0302, 0.0307, 0.0311, 0.0316, 0.0321, 0.0326,
        0.0331, 0.0336, 0.0341, 0.0346, 0.0351, 0.0357, 0.0362, 0.0368,
        0.0373, 0.0379, 0.0385, 0.0390, 0.0396, 0.0402, 0.0408, 0.0415,
        0.0421, 0.0427, 0.0434, 0.0440, 0.0447, 0.0454, 0.0460, 0.0467,
        0.0474, 0.0481, 0.0489, 0.0496, 0.0503, 0.0511, 0.0518, 0.0526,
        0.0534, 0.0542, 0.0550, 0.0558, 0.0567, 0.0575, 0.0583, 0.0592,
        0.0601, 0.0610, 0.0619, 0.0628, 0.0637, 0.0647, 0.0656, 0.0666,
        0.0675, 0.0685, 0.0695, 0.0706, 0.0716, 0.0726, 0.0737, 0.0748,
        0.0759, 0.0770, 0.0781, 0.0792, 0.0804, 0.0815, 0.0827, 0.0839,
        0.0851, 0.0863, 0.0876, 0.0888, 0.0901, 0.0914, 0.0927, 0.0940,
        0.0953, 0.0967, 0.0981, 0.0995, 0.1009, 0.1023, 0.1037, 0.1052,
        0.1067, 0.1082, 0.1097, 0.1112, 0.1128, 0.1144, 0.1160, 0.1176,
        0.1192, 0.1209, 0.1225, 0.1242, 0.1259, 0.1277, 0.1294, 0.1312,
        0.1330, 0.1348, 0.1366, 0.1385, 0.1403, 0.1422, 0.1441, 0.1461,
        0.1480, 0.1500, 0.1520, 0.1541, 0.1561, 0.1582, 0.1603, 0.1624,
        0.1645, 0.1667, 0.1689, 0.1711, 0.1733, 0.1755, 0.1778, 0.1801,
        0.1824, 0.1848, 0.1871, 0.1895, 0.1919, 0.1944, 0.1968, 0.1993,
        0.2018, 0.2043, 0.2069, 0.2095, 0.2121, 0.2147, 0.2173, 0.2200,
        0.2227, 0.2254, 0.2282, 0.2309, 0.2337, 0.2365, 0.2393, 0.2422,
        0.2451, 0.2480, 0.2509, 0.2539, 0.2568, 0.2598, 0.2628, 0.2659,
        0.2689, 0.2720, 0.2751, 0.2783, 0.2814, 0.2846, 0.2878, 0.2910,
        0.2942, 0.2975, 0.3007, 0.3040, 0.3074, 0.3107, 0.3141, 0.3174,
        0.3208, 0.3242, 0.3277, 0.3311, 0.3346, 0.3381, 0.3416, 0.3451,
        0.3486, 0.3522, 0.3558, 0.3594, 0.3630, 0.3666, 0.3702, 0.3739,
        0.3775, 0.3812, 0.3849, 0.3886, 0.3923, 0.3961, 0.3998, 0.4036,
        0.4073, 0.4111, 0.4149, 0.4187, 0.4225, 0.4263, 0.4301, 0.4340,
        0.4378, 0.4417, 0.4455, 0.4494, 0.4533, 0.4571, 0.4610, 0.4649,
        0.4688, 0.4727, 0.4766, 0.4805, 0.4844, 0.4883, 0.4922, 0.4961,
        0.5000, 0.5039, 0.5078, 0.5117, 0.5156, 0.5195, 0.5234, 0.5273,
        0.5312, 0.5351, 0.5390, 0.5429, 0.5467, 0.5506, 0.5545, 0.5583,
        0.5622, 0.5660, 0.5699, 0.5737, 0.5775, 0.5813, 0.5851, 0.5889,
        0.5927, 0.5964, 0.6002, 0.6039, 0.6077, 0.6114, 0.6151, 0.6188,
        0.6225, 0.6261, 0.6298, 0.6334, 0.6370, 0.6406, 0.6442, 0.6478,
        0.6514, 0.6549, 0.6584, 0.6619, 0.6654, 0.6689, 0.6723, 0.6758,
        0.6792, 0.6826, 0.6859, 0.6893, 0.6926, 0.6960, 0.6993, 0.7025,
        0.7058, 0.7090, 0.7122, 0.7154, 0.7186, 0.7217, 0.7249, 0.7280,
        0.7311, 0.7341, 0.7372, 0.7402, 0.7432, 0.7461, 0.7491, 0.7520,
        0.7549, 0.7578, 0.7607, 0.7635, 0.7663, 0.7691, 0.7718, 0.7746,
        0.7773, 0.7800, 0.7827, 0.7853, 0.7879, 0.7905, 0.7931, 0.7957,
        0.7982, 0.8007, 0.8032, 0.8056, 0.8081, 0.8105, 0.8129, 0.8152,
        0.8176, 0.8199, 0.8222, 0.8245, 0.8267, 0.8289, 0.8311, 0.8333,
        0.8355, 0.8376, 0.8397, 0.8418, 0.8439, 0.8459, 0.8480, 0.8500,
        0.8520, 0.8539, 0.8559, 0.8578, 0.8597, 0.8615, 0.8634, 0.8652,
        0.8670, 0.8688, 0.8706, 0.8723, 0.8741, 0.8758, 0.8775, 0.8791,
        0.8808, 0.8824, 0.8840, 0.8856, 0.8872, 0.8888, 0.8903, 0.8918,
        0.8933, 0.8948, 0.8963, 0.8977, 0.8991, 0.9005, 0.9019, 0.9033,
        0.9047, 0.9060, 0.9073, 0.9086, 0.9099, 0.9112, 0.9124, 0.9137,
        0.9149, 0.9161, 0.9173, 0.9185, 0.9196, 0.9208, 0.9219, 0.9230,
        0.9241, 0.9252, 0.9263, 0.9274, 0.9284, 0.9294, 0.9305, 0.9315,
        0.9325, 0.9334, 0.9344, 0.9353, 0.9363, 0.9372, 0.9381, 0.9390,
        0.9399, 0.9408, 0.9417, 0.9425, 0.9433, 0.9442, 0.9450, 0.9458,
        0.9466, 0.9474, 0.9482, 0.9489, 0.9497, 0.9504, 0.9511, 0.9519,
        0.9526, 0.9533, 0.9540, 0.9546, 0.9553, 0.9560, 0.9566, 0.9573,
        0.9579, 0.9585, 0.9592, 0.9598, 0.9604, 0.9610, 0.9615, 0.9621,
        0.9627, 0.9632, 0.9638, 0.9643, 0.9649, 0.9654, 0.9659, 0.9664,
        0.9669, 0.9674, 0.9679, 0.9684, 0.9689, 0.9693, 0.9698, 0.9702,
        0.9707, 0.9711, 0.9716, 0.9720, 0.9724, 0.9728, 0.9732, 0.9736,
        0.9740, 0.9744, 0.9748, 0.9752, 0.9756, 0.9759, 0.9763, 0.9767,
        0.9770, 0.9774, 0.9777, 0.9781, 0.9784, 0.9787, 0.9790, 0.9794,
        0.9797, 0.9800, 0.9803, 0.9806, 0.9809, 0.9812, 0.9815, 0.9817,
    };
    public static readonly IFunc Ω = New();
    public static IFunc New() { return new SigQ(); }
    public override string ToString() { return "ƒ(a) = 1 / (1 + e⁻ᵃ)"; }
    public static double f(double a) {
        if (a < -SIGMOID) {
            return 0.0;
        } else if (a > SIGMOID) {
            return 1.0;
        } else {
            int i = (int)(((a / SIGMOID) + 1) / 2 * _TABLE_SIZE);
            return _TABLE[i];
        }
    }
    public static double df(double f) {
        return f * (1 - f);
    }
    double IFunc.f(double a) {
        return f(a);
    }
    double IFunc.df(double f) {
        return df(f);
    }
}
