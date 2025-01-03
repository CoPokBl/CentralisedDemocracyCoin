using DemCoinCommons;
using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemCoinDiscord.Commands;

public class ImportWalletCommand {
    
    [SlashCommand("import-wallet", "Import your wallet, either fully, or just its address.")]
    [SlashCommandArgument("address", "Your wallet's public address.", false, ApplicationCommandOptionType.String)]
    [SlashCommandArgument("xml", "Your exported wallet info.", false, ApplicationCommandOptionType.String)]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        UserWalletInfo? info = Program.Database.GetUserWallet(cmd.User.Id);
        if (info is { Xml: not null }) {
            // They have a wallet
            await cmd.RespondWithEmbedAsync("Wallet",
                "You cannot import a new wallet because you already have one and it would be overwritten.",
                ResponseType.Error, ephemeral:true);
            return;
        }

        string? address = cmd.GetArgument<string>("address");
        string? xml = cmd.GetArgument<string>("xml");

        if ((xml == null && address == null) || (xml != null && address != null)) {
            await cmd.RespondWithEmbedAsync("Wallet", "You must provide exactly ONE of either an address or XML.", ResponseType.Error, ephemeral:true);
            return;
        }

        info = new UserWalletInfo(cmd.User.Id, null, null);
        if (address == null) {  // full import
            try {
                DemCoinWallet wallet = DemCoinWallet.Import(xml!);
                info.Address = wallet.Address;
                info.Xml = wallet.Export();
            }
            catch (Exception) {
                await cmd.RespondWithEmbedAsync("Wallet", "Invalid wallet XML, could not import.", ResponseType.Error, ephemeral:true);
                return;
            }
        }
        else {  // just address
            try {
                byte[] _ = Convert.FromBase64String(address);
                info.Address = address;
            }
            catch (Exception) {
                await cmd.RespondWithEmbedAsync("Wallet", "Invalid wallet address, could not import.", ResponseType.Error, ephemeral:true);
                return;
            }
        }
        
        Program.Database.SetUserWallet(info);
        await cmd.RespondWithEmbedAsync("Wallet", "Your wallet has been successfully imported.", ResponseType.Success, ephemeral:true);
    }
}