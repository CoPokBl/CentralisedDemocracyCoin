using DemCoinCommons;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemCoinDiscord.Commands;

public class CreateWalletCommand {
    
    [SlashCommand("create-wallet", "Generate a new wallet.")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        UserWalletInfo? info = Program.Database.GetUserWallet(cmd.User.Id);
        if (info is { Xml: not null }) {
            // They have a wallet
            await cmd.RespondWithEmbedAsync("Wallet",
                "You cannot generate a new wallet because you already have one and it would be overwritten.",
                ResponseType.Error);
            return;
        }
        
        DemCoinWallet wallet = DemCoinWallet.New();
        info = new UserWalletInfo(cmd.User.Id, wallet.Address, wallet.Export());
        
        Program.Database.SetUserWallet(info);
        await cmd.RespondWithEmbedAsync("Wallet", "A new wallet has been successfully generated for you.", ResponseType.Success);
    }
}