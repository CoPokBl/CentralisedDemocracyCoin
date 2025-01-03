using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemCoinDiscord.Commands;

public class RemoveWalletCommand {
    
    [SlashCommand("remove-wallet", "Remove our record of your wallet from our systems. MAKE SURE YOU EXPORT IT FIRST.")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        UserWalletInfo? info = Program.Database.GetUserWallet(cmd.User.Id);
        if (info?.Address == null) {
            // They don't have a wallet
            await cmd.RespondWithEmbedAsync("Wallet",
                "We don't have any records of your wallet.",
                ResponseType.Error);
            return;
        }

        Program.Database.SetUserWallet(new UserWalletInfo(cmd.User.Id, null, null));
        await cmd.RespondWithEmbedAsync("Wallet", 
            "All records we have of your wallet have been removed from our systems",
            ResponseType.Success);
    }
}