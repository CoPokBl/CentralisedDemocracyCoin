using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;

namespace DemCoinCommons;

public class DemCoinUtils {
    /// <summary>
    /// Use to rent byte arrays to use less memory.
    /// </summary>
    private static readonly ArrayPool<byte> ByteArrayProvider = ArrayPool<byte>.Create(40, 5);
    
    public static bool IsNonceValid(byte[] prevHash, byte[] nonce) {
        byte[] data = ByteArrayProvider.Rent(prevHash.Length + nonce.Length);  // should be len 40
        Debug.Assert(data.Length == prevHash.Length + nonce.Length, "Array is correct size, is: " + prevHash.Length + nonce.Length);
        Array.Copy(prevHash, data, prevHash.Length);
        Array.Copy(nonce, 0, data, prevHash.Length, nonce.Length);
        byte[] hash = ByteArrayProvider.Rent(32);
        SHA256.HashData(data, hash);
        ByteArrayProvider.Return(data);
        bool valid = IsHashValidBlock(hash);
        ByteArrayProvider.Return(hash);
        return valid;
    }
    
    /// <summary>
    /// Check if a nonce is valid, use existing buffers for efficiency.
    /// </summary>
    /// <param name="prevHash"></param>
    /// <param name="nonce"></param>
    /// <param name="dataBuff"></param>
    /// <param name="hashBuff"></param>
    /// <returns></returns>
    public static bool IsNonceValid(byte[] prevHash, byte[] nonce, byte[] dataBuff, byte[] hashBuff) {
        Debug.Assert(dataBuff.Length == prevHash.Length + nonce.Length);
        Array.Copy(prevHash, dataBuff, prevHash.Length);
        Array.Copy(nonce, 0, dataBuff, prevHash.Length, nonce.Length);
        SHA256.HashData(dataBuff, hashBuff);
        bool valid = IsHashValidBlock(hashBuff);
        return valid;
    }
    
    private static bool IsHashValidBlock(IReadOnlyList<byte> hash) {
        for (byte i = 0; i < DemCoinSettings.Difficulty; i++) {
            if ((hash[i/8] & (0b10000000 >> (i%8))) != 0) {
                return false;
            }
        }

        return true;
    }
}