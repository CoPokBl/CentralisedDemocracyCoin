namespace DemCoinCommons;

public class DemCoinSettings {
    // 23 is very fast, 32 is reasonable for prod
    public const int Difficulty = 33;  // Number of leading zeroes (bits) required in hash
    public const double MinerReward = 1;  // Coins rewarded for mining one block
    public static double AverageHashesToBlock() => Math.Pow(2, Difficulty);  // Average amount of tries before mining one block
}
