namespace DemCoinCommons;

public class GenericMiner(byte[] prevHash) {
    public byte[] PrevHash = prevHash;
    public event Action<byte[]> MinedBlock;
    public ulong CheckedNonces;

    public void MineSync(CancellationToken? token = null) {
        token ??= CancellationToken.None;
        MinerThread(token.Value);
    }
    
    public void MineAsync(CancellationToken token, int threads = 1) {
        for (int i = 0; i < threads; i++) {
            Thread thread = new(() => MinerThread(token));
            thread.Start();
        }
    }
    
    private void MinerThread(CancellationToken cancelToken) {
        Random random = new();

        byte[] nonce = new byte[8];
        byte[] dataBuff = new byte[40];
        byte[] hashBuff = new byte[32];
        while (!cancelToken.IsCancellationRequested) { 
            random.NextBytes(nonce);
            CheckedNonces++;
            if (DemCoinUtils.IsNonceValid(PrevHash, nonce, dataBuff, hashBuff)) {  // We mined a block
                MinedBlock.Invoke(nonce);
            }
        }
    }
}