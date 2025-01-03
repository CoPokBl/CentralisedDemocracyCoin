using CoinBackend.Crypto;
using GeneralPurposeLib;
using LogLevel = GeneralPurposeLib.LogLevel;

namespace CoinBackend;

public class Program {
    public static DemCoinNode CoinNode = new();
    
    public static void Main(string[] args) {
        Logger.Init(LogLevel.Debug);
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.MapControllers();
        
        CoinNode.Init();

        app.Run();
    }
}