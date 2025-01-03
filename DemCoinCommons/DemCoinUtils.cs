using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using NSec.Cryptography;
using Org.BouncyCastle.Crypto.Digests;
using HashAlgorithm = NSec.Cryptography.HashAlgorithm;

namespace DemCoinCommons;

public class DemCoinUtils {
    /// <summary>
    /// Use to rent byte arrays to use less memory.
    /// </summary>
    private static readonly ArrayPool<byte> ByteArrayProvider = ArrayPool<byte>.Create(40, 5);
    
    /// <summary>
    /// Slower legacy nonce validation function. Don't use this for mining.
    /// </summary>
    /// <param name="prevHash"></param>
    /// <param name="nonce"></param>
    /// <returns></returns>
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
    /// BOUNCYCASTLE HASHING.
    /// Check if a nonce is valid, use existing buffers for efficiency.
    ///
    /// This is a highly efficient function for calling repeatedly.
    /// Just make sure all the buffers and SHA256 digest are reused.
    /// </summary>
    /// <param name="prevHash">The previous block's hash.</param>
    /// <param name="nonce">The nonce to verify. (Random data)</param>
    /// <param name="dataBuff">A reused byte buffer of length prevHash.len + nonce.len.</param>
    /// <param name="hashBuff">A reused byte buffer of length 32.</param>
    /// <param name="sha256">The reused SHA256 BouncyCastle object.</param>
    /// <returns>Whether the nonce is valid.</returns>
    public static bool IsNonceValidBouncy(byte[] prevHash, byte[] nonce, byte[] dataBuff, byte[] hashBuff, Sha256Digest sha256) {
        // Debug.Assert(dataBuff.Length == prevHash.Length + nonce.Length);  Optimise for speed
        Array.Copy(prevHash, dataBuff, prevHash.Length);
        Array.Copy(nonce, 0, dataBuff, prevHash.Length, nonce.Length);
        
        sha256.BlockUpdate(dataBuff, 0, dataBuff.Length);  // 4.38% of CPU time
        sha256.DoFinal(hashBuff, 0);                      // 86.3% of CPU time
        
        return IsHashValidBlock(hashBuff);
    }

    /// <summary>
    /// NSEC HASHING.
    /// Check if a nonce is valid, use existing buffers for efficiency.
    ///
    /// This is a highly efficient function for calling repeatedly.
    /// Just make sure all the buffers and SHA256 digest are reused.
    /// </summary>
    /// <param name="prevHash">The previous block's hash.</param>
    /// <param name="nonce">The nonce to verify. (Random data)</param>
    /// <param name="dataBuff">A reused byte buffer of length prevHash.len + nonce.len.</param>
    /// <param name="hashBuff">A reused byte buffer of length 32.</param>
    /// <param name="sha256">The reused SHA256 BouncyCastle object.</param>
    /// <returns>Whether the nonce is valid.</returns>
    public static bool IsNonceValid(byte[] prevHash, byte[] nonce, byte[] dataBuff, byte[] hashBuff) {
        Array.Copy(prevHash, dataBuff, prevHash.Length);
        Array.Copy(nonce, 0, dataBuff, prevHash.Length, nonce.Length);

        HashAlgorithm.Sha256.Hash(dataBuff, hashBuff);
        
        return IsHashValidBlock(hashBuff);
    }
    
    /// <summary>
    /// Checks if the hash starts with enough zero bits.
    ///
    /// This function uses about 3% of runtime. Please refer to the Testing project to try and optimise this.
    /// But this seems to be very efficient and IsNonceValid is more of a slow point.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    private static bool IsHashValidBlock(IReadOnlyList<byte> hash) {
        for (byte i = 0; i < DemCoinSettings.Difficulty; i++) {
            if ((hash[i/8] & (0b10000000 >> (i%8))) != 0) {
                return false;
            }
        }

        return true;
    }
}