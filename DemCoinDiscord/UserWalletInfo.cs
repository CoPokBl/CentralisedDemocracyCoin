using DemCoinCommons;

namespace DemCoinDiscord;

public class UserWalletInfo(ulong userId, string? address, string? xml) {
    public ulong UserId = userId;
    public string? Address = address;
    public string? Xml = xml;

    public DemCoinWallet? Wallet => Xml == null ? null : DemCoinWallet.Import(Xml);
    public byte[] PublicKey => Convert.FromBase64String(Address!);
}