using DemCoinCommons;
using Discord;
using Discord.WebSocket;
using Miner;
using SimpleDiscordNet.Commands;

namespace DemCoinDiscord.Commands;

public class TransferCoinsCommand {
    
    [SlashCommand("transfer-coins", "Send another user some coins.")]
    [SlashCommandArgument("recipient", "The recipient of your funds.", true, ApplicationCommandOptionType.User)]
    [SlashCommandArgument("amount", "The amount of currency to send.", true, ApplicationCommandOptionType.Number)]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        UserWalletInfo? info = Program.Database.GetUserWallet(cmd.User.Id);
        if (info?.Xml == null) {
            // They have a wallet
            await cmd.RespondWithEmbedAsync("Wallet",
                "We can't make transactions for you because you haven't imported your wallet.",
                ResponseType.Error);
            return;
        }

        double amount = cmd.GetArgument<double>("amount");
        IUser recipient = cmd.GetArgument<IUser>("recipient")!;

        UserWalletInfo? recipientInfo = Program.Database.GetUserWallet(recipient.Id);
        if (recipientInfo?.Address == null) {
            await cmd.RespondWithEmbedAsync("Wallet",
                "Your specified recipient hasn't set up their wallet yet.",
                ResponseType.Error);
            return;
        }

        await cmd.DeferAsync();

        WalletBalance bal = await CoinApi.GetWalletBalance(info.PublicKey);
        if (bal.balance < amount) {
            await cmd.ModifyWithEmbedAsync("Wallet", $"You have insufficient funds. Balance: {bal.balance}.", ResponseType.Error);
            return;
        }

        Transaction transaction = new() {
            TransactionNumber = bal.nextTransactionNumber,
            Amount = amount,
            Recipient = recipientInfo.PublicKey,
            Sender = info.PublicKey
        };
        transaction.Sign(info.Wallet!);
        bool success = await CoinApi.SubmitTransaction(transaction);

        if (success) {
            await cmd.ModifyWithEmbedAsync("Wallet",
                "The transaction has been submitted. It may take up to an hour to be processed.", ResponseType.Success);
        }
        else {
            await cmd.ModifyWithEmbedAsync("Wallet",
                "The transaction has been rejected. Please try again later.", ResponseType.Error);
        }
    }
}