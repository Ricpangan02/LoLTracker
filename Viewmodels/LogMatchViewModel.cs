using LoLTracker.Models;
using LoLTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace LoLTracker.ViewModels
{
    public class LogMatchViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        public ObservableCollection<string> Champions { get; }
        public ObservableCollection<string> PlayerNames { get; } = new();

        private string _playerName = string.Empty;
        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        private string _selectedChampion = string.Empty;
        public string SelectedChampion
        {
            get => _selectedChampion;
            set
            {
                _selectedChampion = value;
                OnPropertyChanged();
            }
        }

        private bool _isWin = true;
        public bool IsWin
        {
            get => _isWin;
            set
            {
                _isWin = value;
                OnPropertyChanged();
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddMatchCommand { get; }

        public LogMatchViewModel(DatabaseService db, ChampionService championService)
        {
            _db = db;
            Champions = new ObservableCollection<string>(championService.GetAllChampions());
            if (Champions.Count > 0) SelectedChampion = Champions[0];

            LoadPlayerNames();

            AddMatchCommand = new RelayCommand(AddMatch, CanAddMatch);
        }

        public void LoadPlayerNames()
        {
            var players = _db.GetAllPlayerNames();
            PlayerNames.Clear();
            
            // Sort and add players
            var sorted = players.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var player in sorted)
            {
                if (!string.IsNullOrWhiteSpace(player))
                {
                    PlayerNames.Add(player);
                }
            }
            
            // Set default player if none selected and list is not empty
            if (PlayerNames.Count > 0 && string.IsNullOrWhiteSpace(PlayerName))
            {
                PlayerName = PlayerNames[0];
            }
        }

        private bool CanAddMatch(object? _) => !string.IsNullOrWhiteSpace(SelectedChampion) && !string.IsNullOrWhiteSpace(PlayerName);

        private void AddMatch(object? _)
        {
            if (string.IsNullOrWhiteSpace(PlayerName))
            {
                return;
            }

            var match = new MatchRecord
            {
                PlayerName = PlayerName.Trim(),
                Champion = SelectedChampion,
                IsWin = IsWin,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                Date = DateTime.Now
            };

            _db.InsertMatch(match);

            // Add new player name to list if it doesn't exist
            var trimmedName = PlayerName.Trim();
            if (!PlayerNames.Contains(trimmedName))
            {
                // Find the correct position to insert (keep sorted)
                int insertIndex = 0;
                for (int i = 0; i < PlayerNames.Count; i++)
                {
                    if (string.Compare(PlayerNames[i], trimmedName, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        insertIndex = i;
                        break;
                    }
                    insertIndex = i + 1;
                }
                PlayerNames.Insert(insertIndex, trimmedName);
            }

            // Update PlayerName to the trimmed version
            PlayerName = trimmedName;

            // reset notes
            Notes = string.Empty;
            OnPropertyChanged(nameof(Notes));
        }
    }
}
