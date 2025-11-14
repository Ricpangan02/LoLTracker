using LoLTracker.Models;
using LoLTracker.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoLTracker.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        public int TotalWins { get; private set; }
        public int TotalLosses { get; private set; }
        public int TotalGames => TotalWins + TotalLosses;
        public double OverallWinRate => TotalGames > 0 ? (TotalWins / (double)TotalGames) * 100 : 0;
        public string BestChampion { get; private set; } = "None";
        public string WorstChampion { get; private set; } = "None";

        public ObservableCollection<ChampionStats> ChampionStats { get; private set; } = new();
        public ObservableCollection<PlayerStats> PlayerStats { get; private set; } = new();

        public DashboardViewModel(DatabaseService db)
        {
            _db = db;
            LoadData();
        }

        public void LoadData()
        {
            var matches = _db.GetAllMatches();

            // Overall stats
            TotalWins = matches.Count(m => m.IsWin);
            TotalLosses = matches.Count(m => !m.IsWin);
            OnPropertyChanged(nameof(TotalWins));
            OnPropertyChanged(nameof(TotalLosses));
            OnPropertyChanged(nameof(TotalGames));
            OnPropertyChanged(nameof(OverallWinRate));

            // Champion stats
            var championStats = matches
                .GroupBy(m => m.Champion)
                .Select(g => new ChampionStats
                {
                    Champion = g.Key,
                    Wins = g.Count(x => x.IsWin),
                    Losses = g.Count(x => !x.IsWin)
                })
                .OrderByDescending(s => s.Wins - s.Losses)
                .ToList();

            ChampionStats = new ObservableCollection<ChampionStats>(championStats);
            OnPropertyChanged(nameof(ChampionStats));

            BestChampion = championStats.OrderByDescending(s => s.Wins).FirstOrDefault()?.Champion ?? "None";
            WorstChampion = championStats.OrderByDescending(s => s.Losses).FirstOrDefault()?.Champion ?? "None";
            OnPropertyChanged(nameof(BestChampion));
            OnPropertyChanged(nameof(WorstChampion));

            // Player stats
            var playerStats = matches
                .GroupBy(m => m.PlayerName)
                .Select(g =>
                {
                    var playerMatches = g.ToList();
                    var wins = playerMatches.Count(x => x.IsWin);
                    var losses = playerMatches.Count(x => !x.IsWin);
                    
                    // Calculate best and worst champion for this player
                    var playerChampionStats = playerMatches
                        .GroupBy(m => m.Champion)
                        .Select(cg => new
                        {
                            Champion = cg.Key,
                            Wins = cg.Count(x => x.IsWin),
                            Losses = cg.Count(x => !x.IsWin)
                        })
                        .ToList();
                    
                    var bestChamp = playerChampionStats
                        .OrderByDescending(c => c.Wins)
                        .FirstOrDefault()?.Champion ?? "None";
                    
                    var worstChamp = playerChampionStats
                        .OrderByDescending(c => c.Losses)
                        .FirstOrDefault()?.Champion ?? "None";

                    return new PlayerStats
                    {
                        PlayerName = g.Key,
                        Wins = wins,
                        Losses = losses,
                        BestChampion = bestChamp,
                        WorstChampion = worstChamp
                    };
                })
                .OrderByDescending(p => p.WinRate)
                .ThenByDescending(p => p.GamesPlayed)
                .ToList();

            PlayerStats = new ObservableCollection<PlayerStats>(playerStats);
            OnPropertyChanged(nameof(PlayerStats));
        }
    }
}
