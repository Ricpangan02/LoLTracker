using LoLTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

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
        public ICommand ImportDataCommand { get; }

        public SettingsViewModel(DatabaseService db, Action? onSaved = null)
        {
            _db = db;
            _onSaved = onSaved ?? (() => { });
            ClearAllCommand = new RelayCommand(ClearAll);
            SaveCommand = new RelayCommand(SaveSettings);
            AddPlayerCommand = new RelayCommand(AddPlayer, CanAddPlayer);
            ImportDataCommand = new RelayCommand(ImportData);
            
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

        private void ImportData(object? _)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Import Match Data"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var (imported, failed, errors) = _db.ImportMatchesFromExcel(openFileDialog.FileName);
                    
                    var message = $"Import completed!\n\n" +
                                 $"✓ Successfully imported: {imported} matches\n" +
                                 $"✗ Failed: {failed} matches";

                    if (errors.Count > 0)
                    {
                        var errorDetails = string.Join("\n", errors.Take(10));
                        if (errors.Count > 10)
                        {
                            errorDetails += $"\n... and {errors.Count - 10} more errors";
                        }
                        message += $"\n\nErrors:\n{errorDetails}";
                    }

                    MessageBox.Show(message, 
                                   imported > 0 ? "Import Successful" : "Import Completed", 
                                   MessageBoxButton.OK, 
                                   imported > 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing data: {ex.Message}", 
                                   "Import Error", 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Error);
                }
            }
        }
    }
}
