using Discord;
using GeneralPurposeLib;
using SimpleDiscordNet;

namespace DemCoinDiscord;

public static class Program {
    public static Database Database;
    
    public static async Task Main(string[] args) {
        Logger.Init(LogLevel.Debug);
        GlobalConfig.Init(new Config(DefaultConfig.Config));

        Database = new Database("demcoinbot.db");

        SimpleDiscordBot bot = new(GlobalConfig.Config["token"]);

        bot.Client.Ready += async () => {
            Logger.Info("Bot started");
            bot.UpdateCommands();
            await bot.Client.SetStatusAsync(UserStatus.Online);
            await bot.Client.SetActivityAsync(new Game("Democracy Coin"));
        };

        await bot.StartBot();
        await bot.WaitAsync();
    }
}