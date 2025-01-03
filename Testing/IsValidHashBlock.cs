namespace Testing;
using System.Diagnostics;
using DemCoinCommons;

public class IsValidHashBlock {
    
    private static bool IsHashValidBlock(IReadOnlyList<byte> hash) {
        for (byte i = 0; i < DemCoinSettings.Difficulty; i++) {
            if ((hash[i/8] & (0b10000000 >> (i%8))) != 0) {
                return false;
            }
        }

        return true;
    }

    private static bool IsHashValidBlock2(IReadOnlyList<byte> hash) {
        const int diff = DemCoinSettings.Difficulty;
        for (int i = 0; i < diff; i++) {
            if ((hash[i >> 3] & (0x80 >> (i & 7))) != 0) {
                return false;
            }
        }
        return true;
    }

    private static bool IsHashValidBlock3(IReadOnlyList<byte> hash) {
        // The local variable is a tiny bit faster than the const lookup
        // ReSharper disable once ConvertToConstant.Local
        int diff = DemCoinSettings.Difficulty;
        for (int i = 0; i < diff; i++) {
            if ((hash[i >> 3] & (0x80 >> (i & 7))) != 0) {
                return false;
            }
        }
        return true;
    }

    public static void Run() {
        for (int j = 0; j < 3; j++) {
            Stopwatch stopwatch = new();

            byte[] buff = new byte[32];
            new Random().NextBytes(buff);

            stopwatch.Start();
            for (int i = 0; i < 50_000_000; i++) {
                IsHashValidBlock(buff);
            }

            stopwatch.Stop();
            Console.WriteLine("Took: " + stopwatch.Elapsed);
        }
    }
}