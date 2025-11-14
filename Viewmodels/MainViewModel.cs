using LoLTracker.Services;
using System.Windows.Input;

namespace LoLTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private readonly ChampionService _championService;

        public object CurrentView { get; set; }
        
        private string _summonerName = "Unknown";
        public string SummonerName 
        { 
            get => _summonerName; 
            set 
            { 
                _summonerName = value; 
                OnPropertyChanged();
            } 
        }

        public ICommand GoDashboard { get; }
        public ICommand GoLogMatch { get; }
        public ICommand GoHistory { get; }
        public ICommand GoChampions { get; }
        public ICommand GoSettings { get; }

        public MainViewModel()
        {
            _db = new DatabaseService();
            _championService = new ChampionService();

            // Load saved summoner name
            var savedName = _db.GetSetting("SummonerName");
            if (!string.IsNullOrEmpty(savedName))
            {
                SummonerName = savedName;
            }

            GoDashboard = new RelayCommand(() =>
            {
                var dashboard = new DashboardViewModel(_db);
                dashboard.LoadData(); // Ensure fresh data
                CurrentView = dashboard;
                OnPropertyChanged(nameof(CurrentView));
            });

            GoLogMatch = new RelayCommand(() =>
            {
                var logMatch = new LogMatchViewModel(_db, _championService);
                logMatch.LoadPlayerNames(); // Refresh player list
                CurrentView = logMatch;
                OnPropertyChanged(nameof(CurrentView));
            });

            GoHistory = new RelayCommand(() =>
            {
                CurrentView = new MatchHistoryViewModel(_db);
                OnPropertyChanged(nameof(CurrentView));
            });

            GoChampions = new RelayCommand(() =>
            {
                CurrentView = new ChampionListViewModel(_db);
                OnPropertyChanged(nameof(CurrentView));
            });

            GoSettings = new RelayCommand(() =>
            {
                CurrentView = new SettingsViewModel(_db, () => 
                {
                    SummonerName = (CurrentView as SettingsViewModel)?.SummonerName ?? SummonerName;
                    OnPropertyChanged(nameof(SummonerName));
                });
                OnPropertyChanged(nameof(CurrentView));
            });

            // start page
            CurrentView = new DashboardViewModel(_db);
        }
    }
}
