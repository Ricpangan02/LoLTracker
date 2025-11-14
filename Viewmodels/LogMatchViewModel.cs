using LoLTracker.Models;
using LoLTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LoLTracker.ViewModels
{
    public class LogMatchViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        public ObservableCollection<string> Champions { get; }
        public string SelectedChampion { get; set; } = string.Empty;
        public bool IsWin { get; set; } = true;
        public string Notes { get; set; } = string.Empty;

        public ICommand AddMatchCommand { get; }

        public LogMatchViewModel(DatabaseService db, ChampionService championService)
        {
            _db = db;
            Champions = new ObservableCollection<string>(championService.GetAllChampions());
            if (Champions.Count > 0) SelectedChampion = Champions[0];

            AddMatchCommand = new RelayCommand(AddMatch, CanAddMatch);
        }

        private bool CanAddMatch(object? _) => !string.IsNullOrWhiteSpace(SelectedChampion);

        private void AddMatch(object? _)
        {
            var match = new MatchRecord
            {
                Champion = SelectedChampion,
                IsWin = IsWin,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                Date = DateTime.Now
            };

            _db.InsertMatch(match);

            // reset
            Notes = string.Empty;
            OnPropertyChanged(nameof(Notes));
        }
    }
}
