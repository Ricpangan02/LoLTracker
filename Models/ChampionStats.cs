namespace LoLTracker.Models
{
    public class ChampionStats
    {
        public string Champion { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }

        public double WinRate => (Wins + Losses) == 0 ? 0 : (double)Wins / (Wins + Losses) * 100;
    }
}
