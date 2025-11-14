using LoLTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace LoLTracker.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private readonly Action _onSaved;

        private string _summonerName = string.Empty;
        public string SummonerName
        {
            get => _summonerName;
            set
            {
                _summonerName = value;
                OnPropertyChanged();
            }
        }

        private string _newPlayerName = string.Empty;
        public string NewPlayerName
        {
            get => _newPlayerName;
            set
            {
                _newPlayerName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> PlayerNames { get; } = new();
        public ICommand ClearAllCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddPlayerCommand { get; }

        public SettingsViewModel(DatabaseService db, Action? onSaved = null)
        {
            _db = db;
            _onSaved = onSaved ?? (() => { });
            ClearAllCommand = new RelayCommand(ClearAll);
            SaveCommand = new RelayCommand(SaveSettings);
            AddPlayerCommand = new RelayCommand(AddPlayer, CanAddPlayer);
            
            // Load saved summoner name
            var savedName = _db.GetSetting("SummonerName");
            if (!string.IsNullOrEmpty(savedName))
            {
                SummonerName = savedName;
            }

            LoadPlayerNames();
        }

        private void LoadPlayerNames()
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
        }

        private bool CanAddPlayer(object? _) => !string.IsNullOrWhiteSpace(NewPlayerName);

        private void AddPlayer(object? _)
        {
            if (string.IsNullOrWhiteSpace(NewPlayerName))
            {
                return;
            }

            var trimmedName = NewPlayerName.Trim();
            
            // Check if player already exists
            if (PlayerNames.Contains(trimmedName))
            {
                NewPlayerName = string.Empty;
                return; // Player already exists
            }

            // Save to database
            _db.AddPlayer(trimmedName);

            // Insert in sorted order
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
            
            // Clear the input field
            NewPlayerName = string.Empty;
        }

        private void SaveSettings()
        {
            _db.SaveSetting("SummonerName", SummonerName);
            _onSaved();
        }

        private void ClearAll()
        {
            _db.ClearAll();
        }
    }
}
