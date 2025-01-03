
using DemCoinCommons;
using Miner;

Console.WriteLine("Starting wallet...");

DemCoinWallet wallet;

if (!File.Exists("wallet.xml")) {
    Console.WriteLine("No wallet found. Creating one...");
    
    wallet = DemCoinWallet.New();
    File.WriteAllText("wallet.xml", wallet.Export());
    Console.WriteLine("A new wallet has been created.");
}
else {
    string walletXml = File.ReadAllText("wallet.xml");
    wallet = DemCoinWallet.Import(walletXml);
}

Console.WriteLine("Using wallet: " + wallet.Address);

WalletBalance balance = await CoinApi.GetWalletBalance(wallet.PublicKey);
Console.WriteLine("Balance: " + balance.balance);
return 0;