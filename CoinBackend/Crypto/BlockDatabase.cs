using System.Data.SQLite;
using DemCoinCommons;

namespace CoinBackend.Crypto;

public class BlockDatabase {
    
    private const string ConnectionString = "Data Source=";
    private readonly SQLiteConnection _connection;

    private Block? _cachedLastBlock;

    public BlockDatabase(string path) {
        _connection = new SQLiteConnection(ConnectionString + path + ";");
        _connection.Open();
        CreateTables();
    }
    
    private void CreateTables() {
        SQLiteCommand cmd = new(@"
CREATE TABLE IF NOT EXISTS blocks (
    prev_hash VARCHAR(255) PRIMARY KEY, 
    nonce TEXT,
    transactions TEXT
);

CREATE TABLE IF NOT EXISTS transactions (
    sender VARCHAR(255), 
    recipient VARCHAR(255),
    amount DOUBLE,
    signature TEXT,
    transactionNumber NUMERIC
);
", _connection);
        cmd.ExecuteNonQuery();
    }
    
    public ulong GetBlockCount() {
        using SQLiteCommand cmd = new("SELECT COUNT(*) FROM blocks;", _connection);
        return Convert.ToUInt64(cmd.ExecuteScalar()!);
    }
    
    public Block GetLastBlock(int skip = 0) {
        if (skip == 0 && _cachedLastBlock != null) {
            return _cachedLastBlock;
        }
        
        using SQLiteCommand cmd = new("SELECT * FROM blocks ORDER BY ROWID DESC LIMIT 1 OFFSET @skip;", _connection);
        cmd.Parameters.AddWithValue("@skip", skip);
        using SQLiteDataReader reader = cmd.ExecuteReader();
        reader.Read();
        
        byte[] prevHash = Convert.FromBase64String(reader.GetString(0));
        byte[] data = Convert.FromBase64String(reader.GetString(1));
        Transaction[] transactions = Transaction.DeserializeMany(Convert.FromBase64String(reader.GetString(2)));

        Block block = new(prevHash, data, transactions);
        if (skip == 0) {
            _cachedLastBlock = block;
        }
        return block;
    }
    
    public Block? GetBlockByIndex(ulong index) {
        using SQLiteCommand cmd = new("SELECT * FROM blocks ORDER BY ROWID LIMIT 1 OFFSET @index;", _connection);
        cmd.Parameters.AddWithValue("@index", index);
        using SQLiteDataReader reader = cmd.ExecuteReader();
        reader.Read();

        if (!reader.HasRows) {
            return null;
        }
        
        byte[] prevHash = Convert.FromBase64String(reader.GetString(0));
        byte[] data = Convert.FromBase64String(reader.GetString(1));
        Transaction[] transactions = Transaction.DeserializeMany(Convert.FromBase64String(reader.GetString(2)));
        
        return new Block(prevHash, data, transactions);
    }
    
    public Block[] GetBlockRange(ulong start, ulong end) {
        using SQLiteCommand cmd = new("SELECT * FROM blocks ORDER BY ROWID LIMIT @end OFFSET @start;", _connection);
        cmd.Parameters.AddWithValue("@start", start);
        cmd.Parameters.AddWithValue("@end", end - start);
        using SQLiteDataReader reader = cmd.ExecuteReader();
        
        List<Block> blocks = [];
        while (reader.Read()) {
            byte[] prevHash = Convert.FromBase64String(reader.GetString(0));
            byte[] nonce = Convert.FromBase64String(reader.GetString(1));
            Transaction[] transactions = Transaction.DeserializeMany(Convert.FromBase64String(reader.GetString(2)));
            blocks.Add(new Block(prevHash, nonce, transactions));
        }
        
        return blocks.ToArray();
    }
    
    public void InsertBlock(Block block) {
        using SQLiteCommand cmd = new("INSERT INTO blocks (prev_hash, nonce, transactions) VALUES (@prevHash, @data, @trans);", _connection);
        cmd.Parameters.AddWithValue("@prevHash", Convert.ToBase64String(block.PrevHash));
        cmd.Parameters.AddWithValue("@data", Convert.ToBase64String(block.Nonce));
        cmd.Parameters.AddWithValue("@trans", Convert.ToBase64String(Transaction.SerializeMany(block.Transactions)));
        cmd.ExecuteNonQuery();
        
        _cachedLastBlock = null;
    }
    
    public void InsertTransaction(Transaction transaction) {
        using SQLiteCommand cmd = new("INSERT INTO transactions (sender, recipient, amount, signature, transactionNumber) VALUES (@sender, @recipient, @amount, @sig, @tn);", _connection);
        cmd.Parameters.AddWithValue("@sender", Convert.ToBase64String(transaction.Sender));
        cmd.Parameters.AddWithValue("@recipient", Convert.ToBase64String(transaction.Recipient));
        cmd.Parameters.AddWithValue("@amount", transaction.Amount);
        cmd.Parameters.AddWithValue("@sig", Convert.ToBase64String(transaction.Signature));
        cmd.Parameters.AddWithValue("@tn", transaction.TransactionNumber);
        cmd.ExecuteNonQuery();
    }
    
    public double GetBalance(byte[] publicKey) {  // Add up all amounts where publickey is recipient and subtract where publickey is sender
        using SQLiteCommand cmd = new("SELECT * FROM transactions WHERE sender = @sender OR recipient = @recipient;", _connection);
        cmd.Parameters.AddWithValue("@sender", Convert.ToBase64String(publicKey));
        cmd.Parameters.AddWithValue("@recipient", Convert.ToBase64String(publicKey));
        using SQLiteDataReader reader = cmd.ExecuteReader();
        
        double balance = 0;
        while (reader.Read()) {
            double amount = reader.GetDouble(2);
            byte[] sender = Convert.FromBase64String(reader.GetString(0));
            byte[] recipient = Convert.FromBase64String(reader.GetString(1));
            if (sender.SequenceEqual(publicKey)) {
                balance -= amount;
            }
            if (recipient.SequenceEqual(publicKey)) {
                balance += amount;
            }
        }
        
        return balance;
    }

    public ulong GetLastTransactionNumber(byte[] publicKey) {
        using SQLiteCommand cmd = new("SELECT MAX(transactionNumber) FROM transactions WHERE sender = @sender;", _connection);
        cmd.Parameters.AddWithValue("@sender", Convert.ToBase64String(publicKey));
        cmd.Parameters.AddWithValue("@recipient", Convert.ToBase64String(publicKey));
        object? o = cmd.ExecuteScalar();
        return o is DBNull ? 0 : Convert.ToUInt64(o);
    }
    
}