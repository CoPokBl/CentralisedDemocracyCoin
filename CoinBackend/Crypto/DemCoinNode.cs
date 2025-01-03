using System.Diagnostics;
using System.Security.Cryptography;
using DemCoinCommons;
using GeneralPurposeLib;

namespace CoinBackend.Crypto;

public class DemCoinNode {
    public byte[] PrevBlockHash => _blockDatabase.GetLastBlock().Hash();
    
    private BlockDatabase _blockDatabase = null!;
    private readonly List<Transaction> _pendingTransactions = [];  // These are currently volatile.

    private readonly object _pendingTransactionsLock = new();
    private readonly object _mineLock = new();

    public void Init() {
        _blockDatabase = new BlockDatabase("blockchain.db");
        Logger.Info("Database loaded with " + _blockDatabase.GetBlockCount() + " blocks.");

        Debug.Assert(GetDefBlock().HashString() == Block.Deserialize(GetDefBlock().Serialize()).HashString(), "Deserialize/serialize failed");

        if (_blockDatabase.GetBlockCount() == 0) {
            AddChainStartBlock();
        }
        else {
            Logger.Info("Loaded existing chain. Length: " + _blockDatabase.GetBlockCount());
        }
    }
    
    public bool IsNonceValid(IEnumerable<byte> nonce) {
        byte[] hash = SHA256.HashData(PrevBlockHash.Concat(nonce).ToArray());
        return IsHashValidBlock(hash);
    }
    
    public void AddChainStartBlock() {
        _blockDatabase.InsertBlock(GetDefBlock());
    }

    private static Block GetDefBlock() {
        return new Block {
            PrevHash = new byte[32],
            Nonce = new byte[8],
            Transactions = []
        };
    }

    public void MineBlock(byte[] nonce, byte[] walletAddress) {
        lock (_mineLock) {
            if (!IsNonceValid(nonce)) {
                throw new Exception("Invalid nonce.");
            }
        
            // Get all pending transactions
            List<Transaction> transactions = [
                new() {  // Coinbase, we get a reward :)
                    Sender = new byte[32],
                    Recipient = walletAddress,
                    Amount = DemCoinSettings.MinerReward,
                    Signature = [0],
                    TransactionNumber = 0
                }
            ];

            lock (_pendingTransactionsLock) {
                transactions.AddRange(_pendingTransactions);
                _pendingTransactions.Clear();
            }
        
            Block block = new() {
                PrevHash = _blockDatabase.GetLastBlock().Hash(),
                Nonce = nonce,
                Transactions = transactions.ToArray()
            };

            Debug.Assert(ValidateBlock(block), "Valid block to add to database");  // Sanity check to make sure our own checks pass
        
            AddBlockToDatabase(block);
        
            Debug.Assert(ValidateBlock(_blockDatabase.GetLastBlock(), 1), "Block added to database correctly");
        }
    }

    /// <summary>
    /// Creates a transaction sending money to a wallet and queues it to be added to the blockchain
    /// when the next block is mined.
    /// </summary>
    /// <remarks>
    /// Transaction will not be executed until a block is mined.
    /// </remarks>
    /// <param name="wallet">Your RSA creds.</param>
    /// <param name="to">Recipient of funds.</param>
    /// <param name="amount">Amount to transfer.</param>
    public void SendMoney(RSA wallet, byte[] to, double amount) {
        Transaction transaction = new() {
            Sender = wallet.ExportRSAPublicKey(),
            Recipient = to,
            Amount = amount,
            TransactionNumber = GetNextTransactionNumber(wallet.ExportRSAPublicKey())
        };

        byte[] serialized = transaction.Serialize(true);
        byte[] sig = wallet.SignData(serialized, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        transaction.Signature = sig;

        PublishTransaction(transaction);
    }

    public void PublishTransaction(Transaction transaction) {
        Debug.Assert(ValidateTransaction(transaction));
        lock (_pendingTransactionsLock) {
            _pendingTransactions.Add(transaction);
        }
    }
    
    public double GetBalance(byte[] walletAddress) {
        return _blockDatabase.GetBalance(walletAddress);
    }

    public ulong GetNextTransactionNumber(byte[] walletAddress) {
        lock (_pendingTransactionsLock) {  // Check to see if they have any pending transactions
            Transaction? lastPending = _pendingTransactions.LastOrDefault(t => t.Sender.SequenceEqual(walletAddress));
            if (lastPending != null) {
                return lastPending.TransactionNumber + 1;
            }
        }
        return _blockDatabase.GetLastTransactionNumber(walletAddress) + 1;
    }

    private static bool IsHashValidBlock(IReadOnlyList<byte> hash) {
        for (byte i = 0; i < DemCoinSettings.Difficulty; i++) {
            if ((hash[i/8] & (0b10000000 >> (i%8))) != 0) {
                return false;
            }
        }

        return true;
    }

    private void AddBlockToDatabase(Block block) {
        _blockDatabase.InsertBlock(block);
        
        // Transactions
        foreach (Transaction transaction in block.Transactions) {
            _blockDatabase.InsertTransaction(transaction);
        }
    }

    /// <summary>
    /// Checks the validity of a Block.
    /// </summary>
    /// <param name="block">The block to validate.</param>
    /// <param name="skip">
    /// How far back in the database the block is, the age of the block.
    /// Set to 0 if the block is not in the database and this block comes after the last block in the database.
    /// Set to 1 if the being validated is the last block in the database.
    /// It is how many blocks we need to skip before we get to the block that should come before this block.
    /// Defaults to 0.
    /// </param>
    /// <returns>True if the block is valid, otherwise false.</returns>
    private bool ValidateBlock(Block block, int skip = 0) {
        byte[] expectedHash = _blockDatabase.GetLastBlock(skip).Hash();
        if (!block.PrevHash.SequenceEqual(expectedHash)) {
            Logger.Debug("Block verification failed. (PrevHash bad)");
            return false;
        }

        byte[] blockHash = SHA256.HashData(block.PrevHash.Concat(block.Nonce).ToArray());
        if (!IsHashValidBlock(blockHash)) {
            Logger.Debug("Block verification failed. (Nonce bad)");
            return false;
        }

        if (block.Transactions.Length == 0) {
            Logger.Debug("Block verification failed. (No transactions)");
            return false;
        }

        bool allTransactionsValid = true;
        bool recordedCoinbase = false;
        foreach (Transaction transaction in block.Transactions) {
            if (transaction.Sender.SequenceEqual(new byte[32])) {  // coinbase
                if (recordedCoinbase) {  // Only one is allowed
                    Logger.Debug("Block verification failed. (Only 1 coinbase allowed)");
                    allTransactionsValid = false;
                    break;
                }
                recordedCoinbase = true;
                
                if (Math.Abs(transaction.Amount - DemCoinSettings.MinerReward) > 0.01) {
                    Logger.Debug($"Block verification failed. (Invalid miner reward, reward: {transaction.Amount})");
                    allTransactionsValid = false;
                    break;
                }
                
                continue;
            }
            
            // EVERYTHING HERE IS NOT CHECKED FOR COINBASE TRANSACTIONS
            if (skip == 0 && !ValidateTransaction(transaction)) {
                allTransactionsValid = false;
                break;
            }
        }

        if (!allTransactionsValid) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check the validity of a Transaction object that has not been added to the database.
    /// This works will pending transactions.
    /// This method logs the fail reason apon rejection.
    /// </summary>
    /// <remarks>
    /// THIS WILL NOT WORK ON TRANSACTIONS ALREADY IN THE DATABASE. It will fail because the transaction number will be invalid.
    /// </remarks>
    /// <param name="transaction">The transaction to validate, MUST NOT BE IN DATABASE.</param>
    /// <returns>True if the transaction is valid, otherwise false.</returns>
    public bool ValidateTransaction(Transaction transaction) {
        if (transaction.Amount <= 0) {  // Coinbases need a specific amount so this isn't needed
            Logger.Debug($"Block verification failed. (Negative transaction, amount: {transaction.Amount})");
            return false;
        }
            
        // Check if sender has enough money
        double senderBalance = _blockDatabase.GetBalance(transaction.Sender);
        if (senderBalance < transaction.Amount) {
            Logger.Debug($"Block verification failed. (Insufficient funds in transaction, senderbal: {senderBalance}, amount: {transaction.Amount})");
            return false;
        }
            
        // Check if transaction is valid (Not needed for coinbases)
        if (!transaction.IsSignatureValid()) {
            Logger.Debug("Block verification failed. (Invalid transaction signature)");
            return false;
        }
            
        // Check if the transaction number is valid
        ulong nextTn = GetNextTransactionNumber(transaction.Sender);
        if (transaction.TransactionNumber != nextTn) {
            Logger.Debug($"Block verification failed. (Transaction number invalid, expected: {nextTn}, actual: {transaction.TransactionNumber}.");
            return false;
        }

        return true;
    }
    
}