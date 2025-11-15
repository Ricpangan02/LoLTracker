using LoLTracker.Models;
using LoLTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoLTracker.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private string? _selectedPlayer;
        private List<MatchRecord> _allMatches = new();
        private bool _isLoading = false;

        public ObservableCollection<string> PlayerNames { get; } = new();
        
        public string? SelectedPlayer
        {
            get => _selectedPlayer;
            set
            {
                _selectedPlayer = value;
                OnPropertyChanged();
                if (!_isLoading)
                {
                    LoadFilteredData();
                }
            }
        }

        public int TotalWins { get; private set; }
        public int TotalLosses { get; private set; }
        public int TotalGames => TotalWins + TotalLosses;
        public double OverallWinRate => TotalGames > 0 ? (TotalWins / (double)TotalGames) * 100 : 0;
        public double BraveryWinRate { get; private set; }
        public double SelectedWinRate { get; private set; }
        public string BestChampion { get; private set; } = "None";
        public string WorstChampion { get; private set; } = "None";

        public ObservableCollection<ChampionStats> ChampionStats { get; private set; } = new();
        public ObservableCollection<PlayerStats> PlayerStats { get; private set; } = new();

        public DashboardViewModel(DatabaseService db)
        {
            _db = db;
            LoadPlayerNames();
            LoadData();
        }

        private void LoadPlayerNames()
        {
            _isLoading = true;
            var currentSelection = SelectedPlayer;
            var players = _db.GetAllPlayerNames();
            PlayerNames.Clear();
            PlayerNames.Add("All Players"); // Add option to show all players
            foreach (var player in players)
            {
                PlayerNames.Add(player);
            }
            
            // Restore selection if it still exists, otherwise default to "All Players"
            if (string.IsNullOrEmpty(currentSelection) || !PlayerNames.Contains(currentSelection))
            {
                SelectedPlayer = "All Players";
            }
            else
            {
                SelectedPlayer = currentSelection;
            }
            _isLoading = false;
        }

        public void LoadData()
        {
            _allMatches = _db.GetAllMatches();
            LoadPlayerNames(); // Refresh player list in case new players were added
            LoadFilteredData();
            LoadPlayerStats();
        }

        private void LoadFilteredData()
        {
            // Filter matches based on selected player
            var matches = _allMatches;
            if (!string.IsNullOrEmpty(SelectedPlayer) && SelectedPlayer != "All Players")
            {
                matches = matches.Where(m => m.PlayerName == SelectedPlayer).ToList();
            }

            // Stats for selected player (or all players)
            TotalWins = matches.Count(m => m.IsWin);
            TotalLosses = matches.Count(m => !m.IsWin);
            OnPropertyChanged(nameof(TotalWins));
            OnPropertyChanged(nameof(TotalLosses));
            OnPropertyChanged(nameof(TotalGames));
            OnPropertyChanged(nameof(OverallWinRate));

            // Calculate Bravery and Selected win rates
            var braveryMatches = matches.Where(m => m.GameMode == "Bravery").ToList();
            var braveryWins = braveryMatches.Count(m => m.IsWin);
            var braveryGames = braveryMatches.Count;
            BraveryWinRate = braveryGames > 0 ? (braveryWins / (double)braveryGames) * 100 : 0;
            OnPropertyChanged(nameof(BraveryWinRate));

            var selectedMatches = matches.Where(m => m.GameMode == "Selected").ToList();
            var selectedWins = selectedMatches.Count(m => m.IsWin);
            var selectedGames = selectedMatches.Count;
            SelectedWinRate = selectedGames > 0 ? (selectedWins / (double)selectedGames) * 100 : 0;
            OnPropertyChanged(nameof(SelectedWinRate));

            // Champion stats for selected player (or all players)
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
        }

        private void LoadPlayerStats()
        {
            // Player stats (always show all players)
            var playerStats = _allMatches
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
