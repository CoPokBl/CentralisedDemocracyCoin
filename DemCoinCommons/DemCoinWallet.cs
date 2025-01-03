using System.Security.Cryptography;

namespace DemCoinCommons;

public class DemCoinWallet {
    public RSA Creds;

    public byte[] PublicKey => Creds.ExportRSAPublicKey();
    public string Address => Convert.ToBase64String(PublicKey);

    private DemCoinWallet(RSA creds) {
        Creds = creds;
    }

    public static DemCoinWallet Import(string xml) {
        RSA rsa = RSA.Create();
        rsa.FromXmlString(xml);
        return new DemCoinWallet(rsa);
    }

    public static DemCoinWallet New() {
        return new DemCoinWallet(RSA.Create());
    }

    public string Export() {
        return Creds.ToXmlString(true);
    }

    public override string ToString() {
        return Export();
    }
}