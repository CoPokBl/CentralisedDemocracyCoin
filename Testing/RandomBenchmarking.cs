using System.Diagnostics;
using Org.BouncyCastle.Security;

namespace Testing;

public class RandomBenchmarking {
    private const int Tests = 100_000_000;
    private static uint seed = 99999;//(uint)DateTime.Now.Ticks;

    private static void Test(string name, Action generator) {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        for (int i = 0; i < Tests; i++) {
            generator.Invoke();
        }
        stopwatch.Stop();
        Console.WriteLine(name + " random: " + stopwatch.Elapsed);
    }

    public static void Run() {
        Random random = new();
        SecureRandom bouncyRandom = new();
        byte[] buff = new byte[8];

        Test("System", () => random.NextBytes(buff));
        // Test("Bouncy", () => bouncyRandom.NextBytes(buff));
        Test("Custom", () => Ran(buff));
    }

    private static void Ran(byte[] buff) {
        int len = buff.Length;
        for (int i = 0; i < len; i++) {
            buff[i] = NextByte();
        }
    }
    
    private static byte NextByte() {
        return 0x00;
    }
}