using System.Text;
using System.Text.Json;
using DemCoinCommons;

namespace Miner;

public static class CoinApi {
    public static string ApiUrl = "http://zane.serble.net:5208";
    private static readonly HttpClient Client = new();

    public static async Task<MiningStatus> GetMiningStatus() {
        HttpResponseMessage resp = await Client.GetAsync(ApiUrl + "/mining/status");
        if (!resp.IsSuccessStatusCode) {
            throw new Exception("Non success status: " + resp.StatusCode);
        }

        MiningStatus status = JsonSerializer.Deserialize<MiningStatus>(await resp.Content.ReadAsStringAsync())!;
        return status;
    }

    public static async Task<bool> SubmitNonce(byte[] nonce, byte[] wallet) {
        MiningSubmission submission = new(nonce, wallet);
        string json = JsonSerializer.Serialize(submission);
        HttpResponseMessage resp = await Client.PostAsync(ApiUrl + "/mining/submit", new StringContent(json, Encoding.Default, "application/json"));
        return resp.IsSuccessStatusCode;
    }
    
    public static async Task<WalletBalance> GetWalletBalance(byte[] wallet) {
        WalletInfo walletInfo = new(wallet);
        string json = JsonSerializer.Serialize(walletInfo);
        HttpResponseMessage resp = await Client.PostAsync(ApiUrl + "/wallet/balance", new StringContent(json, Encoding.Default, "application/json"));
        if (!resp.IsSuccessStatusCode) {
            throw new Exception("Non success status: " + resp.StatusCode);
        }

        WalletBalance bal = JsonSerializer.Deserialize<WalletBalance>(await resp.Content.ReadAsStringAsync())!;
        return bal;
    }

    public static async Task<bool> SubmitTransaction(Transaction transaction) {
        string json = JsonSerializer.Serialize(transaction);
        HttpResponseMessage resp = await Client.PostAsync(ApiUrl + "/wallet/transfer", new StringContent(json, Encoding.Default, "application/json"));
        return resp.IsSuccessStatusCode;
    }
}