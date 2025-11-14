using LoLTracker.Services;
using System.Windows.Input;

namespace LoLTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private readonly ChampionService _championService;

        public object CurrentView { get; set; }
        public string SummonerName { get; set; } = "Unknown";

        public ICommand GoDashboard { get; }
        public ICommand GoLogMatch { get; }
        public ICommand GoHistory { get; }
        public ICommand GoChampions { get; }
        public ICommand GoSettings { get; }

        public MainViewModel()
        {
            _db = new DatabaseService();
            _championService = new ChampionService();

            GoDashboard = new RelayCommand(() =>
            {
                CurrentView = new DashboardViewModel(_db);
                OnPropertyChanged(nameof(CurrentView));
            });

            GoLogMatch = new RelayCommand(() =>
            {
                CurrentView = new LogMatchViewModel(_db, _championService);
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
                CurrentView = new SettingsViewModel(_db, () => SummonerName = (CurrentView as SettingsViewModel)?.SummonerName ?? SummonerName);
                OnPropertyChanged(nameof(CurrentView));
            });

            // start page
            CurrentView = new DashboardViewModel(_db);
        }
    }
}
