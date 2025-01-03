using DemCoinCommons;
using Discord.WebSocket;
using Miner;
using SimpleDiscordNet.Commands;

namespace DemCoinDiscord.Commands;

public class BalanceCommand {
    
    [SlashCommand("balance", "Check you saved wallet's balance.")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        UserWalletInfo? info = Program.Database.GetUserWallet(cmd.User.Id);
        if (info?.Address == null) {
            // They have a wallet
            await cmd.RespondWithEmbedAsync("Wallet",
                "We can't check your balance because you haven't told us your wallet address.",
                ResponseType.Error);
            return;
        }

        await cmd.DeferAsync();

        WalletBalance bal = await CoinApi.GetWalletBalance(Convert.FromBase64String(info.Address));
        await cmd.ModifyWithEmbedAsync("Wallet", $"**Wallet Balance: {bal.balance} dc**\n\n*Address: {info.Address}*");
    }
}