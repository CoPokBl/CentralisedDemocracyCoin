using System.Data.SQLite;

namespace DemCoinDiscord;

public class Database {
    private const string ConnectionString = "Data Source=";
    private readonly SQLiteConnection _connection;

    public Database(string path) {
        _connection = new SQLiteConnection(ConnectionString + path + ";");
        _connection.Open();
        CreateTables();
    }
    
    private void CreateTables() {
        SQLiteCommand cmd = new(@"
CREATE TABLE IF NOT EXISTS wallets (
    userid VARCHAR(64),
    address TEXT, 
    exported TEXT
);
", _connection);
        cmd.ExecuteNonQuery();
    }
    
    public UserWalletInfo? GetUserWallet(ulong userId) {
        using SQLiteCommand cmd = new("SELECT * FROM wallets WHERE userid = @userid;", _connection);
        cmd.Parameters.AddWithValue("@userid", userId);
        using SQLiteDataReader reader = cmd.ExecuteReader();
        if (!reader.Read()) {
            return null;
        }

        string? address = reader.IsDBNull(1) ? null : reader.GetString(1);
        string? xml = reader.IsDBNull(2) ? null : reader.GetString(2);
        
        return new UserWalletInfo(userId, address, xml);
    }
    
    public void SetUserWallet(UserWalletInfo info) {
        using SQLiteCommand cmd = new(@"
DELETE FROM wallets WHERE userid=@userid;
INSERT INTO wallets (userid, address, exported) VALUES (@userid, @address, @exported);", _connection);
        cmd.Parameters.AddWithValue("@userid", info.UserId);
        cmd.Parameters.AddWithValue("@address", info.Address);
        cmd.Parameters.AddWithValue("@exported", info.Xml);
        cmd.ExecuteNonQuery();
    }
}