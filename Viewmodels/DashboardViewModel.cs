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
        public string BestChampion { get; private set; } = "None";
        public string WorstChampion { get; private set; } = "None";

        public ObservableCollection<ChampionStats> ChampionStats { get; private set; } = new();

        public DashboardViewModel(DatabaseService db)
        {
            _db = db;
            LoadData();
        }

        public void LoadData()
        {
            var matches = _db.GetAllMatches();

            TotalWins = matches.Count(m => m.IsWin);
            TotalLosses = matches.Count(m => !m.IsWin);
            OnPropertyChanged(nameof(TotalWins));
            OnPropertyChanged(nameof(TotalLosses));

            var stats = matches
                .GroupBy(m => m.Champion)
                .Select(g => new ChampionStats
                {
                    Champion = g.Key,
                    Wins = g.Count(x => x.IsWin),
                    Losses = g.Count(x => !x.IsWin)
                })
                .OrderByDescending(s => s.Wins - s.Losses)
                .ToList();

            ChampionStats = new ObservableCollection<ChampionStats>(stats);
            OnPropertyChanged(nameof(ChampionStats));

            BestChampion = stats.OrderByDescending(s => s.Wins).FirstOrDefault()?.Champion ?? "None";
            WorstChampion = stats.OrderByDescending(s => s.Losses).FirstOrDefault()?.Champion ?? "None";
            OnPropertyChanged(nameof(BestChampion));
            OnPropertyChanged(nameof(WorstChampion));
        }
    }
}
