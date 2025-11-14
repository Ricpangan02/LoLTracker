using LoLTracker.Models;
using LoLTracker.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoLTracker.ViewModels
{
    public class ChampionListViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        public ObservableCollection<ChampionStats> ChampionStats { get; private set; } = new();

        public ChampionListViewModel(DatabaseService db)
        {
            _db = db;
            Load();
        }

        public void Load()
        {
            var matches = _db.GetAllMatches();
            var stats = matches
                .GroupBy(m => m.Champion)
                .Select(g => new ChampionStats
                {
                    Champion = g.Key,
                    Wins = g.Count(x => x.IsWin),
                    Losses = g.Count(x => !x.IsWin)
                })
                .OrderByDescending(s => s.Wins)
                .ToList();

            ChampionStats = new ObservableCollection<ChampionStats>(stats);
            OnPropertyChanged(nameof(ChampionStats));
        }
    }
}
