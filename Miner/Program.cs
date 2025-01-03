using DemCoinCommons;
using Miner;

Console.WriteLine("Starting miner...");

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

MiningStatus status = await CoinApi.GetMiningStatus();
GenericMiner miner = new(status.prevBlockHash);

miner.MinedBlock += async nonce => {
    Console.WriteLine("Submitting nonce: " + Convert.ToBase64String(nonce));
    bool success = await CoinApi.SubmitNonce(nonce, wallet.PublicKey);
    
    Console.WriteLine(success ? "MINED BLOCK" : "Nonce rejected");
};

CancellationTokenSource cts = new();

Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    cts.Cancel();
};

miner.MineAsync(cts.Token, Environment.ProcessorCount);

while (!cts.IsCancellationRequested) {
    status = await CoinApi.GetMiningStatus();
    miner.PrevHash = status.prevBlockHash;
    Thread.Sleep(1000);
}

return 0;