using System.Diagnostics;
using System.Security.Cryptography;
using NSec.Cryptography;
using Org.BouncyCastle.Crypto.Digests;
using HashAlgorithm = NSec.Cryptography.HashAlgorithm;

namespace Testing;

public class HashingBenchmarking {
    private const int Tests = 10_000_000;

    private static void Test(string name, Action hasher) {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        for (int i = 0; i < Tests; i++) {
            hasher.Invoke();
        }
        stopwatch.Stop();
        Console.WriteLine(name + " hashing: " + stopwatch.Elapsed);
    }

    public static void Run() {
        Sha256Digest sha256 = new();
        // SodiumCore.Init();

        byte[] buff = new byte[40];
        byte[] result = new byte[32];
        
        // Test("System", () => SHA256.HashData(buff, result));
        Test("Bouncy", () => {
            sha256.BlockUpdate(buff, 0, buff.Length);  // 4.38% of CPU time
            sha256.DoFinal(result, 0);                      // 86.3% of CPU time
        });
        // Test("Sodium", () => result = CryptoHash.Sha256(buff));
        Test("NSec", () => HashAlgorithm.Sha256.Hash(buff, result));
    }
}