namespace DemCoinCommons;

public class DemCoinSettings {
    public const int Difficulty = 23;  // Number of leading zeroes required in hash  was 26, prob 30 for adam, 23 for testing
    public const double MinerReward = 1;  // Coins rewarded for mining one block
    public static int AverageHashesToBlock() => (int)Math.Pow(2, Difficulty);  // Average amount of tries before mining one block
}