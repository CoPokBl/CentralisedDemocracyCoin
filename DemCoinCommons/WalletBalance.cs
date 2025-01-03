namespace DemCoinCommons;

public record WalletBalance(byte[] wallet, double balance, ulong nextTransactionNumber);
