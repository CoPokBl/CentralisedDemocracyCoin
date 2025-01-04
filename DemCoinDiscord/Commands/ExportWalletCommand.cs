using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemCoinDiscord.Commands;

public class ExportWalletCommand {
    
    [SlashCommand("export-wallet", "Get XML that you can use to import you wallet into another application.")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        UserWalletInfo? info = Program.Database.GetUserWallet(cmd.User.Id);
        if (info?.Xml == null) {
            // They don't have a wallet
            await cmd.RespondWithEmbedAsync("Wallet",
                "We can't export your wallet because you haven't set one up, use */balance* to view any saved address.",
                ResponseType.Error);
            return;
        }

        await cmd.RespondAsync($"Here is your exported wallet: ```{info.Wallet!.Export()}```", ephemeral:true);
    }
}