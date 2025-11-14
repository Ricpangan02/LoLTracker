namespace LoLTracker.Models
{
    public class PlayerStats
    {
        public string PlayerName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int GamesPlayed => Wins + Losses;
        public double WinRate => GamesPlayed > 0 ? (Wins / (double)GamesPlayed) * 100 : 0;
        public string BestChampion { get; set; } = "None";
        public string WorstChampion { get; set; } = "None";
    }
}

