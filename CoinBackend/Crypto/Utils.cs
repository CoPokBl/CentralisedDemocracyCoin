using System.Text;

namespace CoinBackend.Crypto;

public static class Utils {
    
    public static string BytesToHex(this IReadOnlyList<byte> bytes) {
        StringBuilder hex = new(bytes.Count * 2);
        foreach (byte b in bytes) {
            hex.Append($"{b:x2}");
        }
        return hex.ToString();
    }

    public static string RepeatChars(char c, int a) {
        StringBuilder sb = new(a);
        for (int i = 0; i < a; i++) {
            sb.Append(c);
        }
        return sb.ToString();
    }
    
}