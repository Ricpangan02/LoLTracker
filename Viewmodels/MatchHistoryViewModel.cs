using LoLTracker.Models;
using LoLTracker.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LoLTracker.ViewModels
{
    public class MatchHistoryViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        public ObservableCollection<MatchRecord> Matches { get; private set; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand DeleteCommand { get; }

        public MatchHistoryViewModel(DatabaseService db)
        {
            _db = db;
            RefreshCommand = new RelayCommand(LoadMatches);
            DeleteCommand = new RelayCommand(DeleteMatch);
            LoadMatches();
        }

        public void LoadMatches()
        {
            Matches = new ObservableCollection<MatchRecord>(_db.GetAllMatches());
            OnPropertyChanged(nameof(Matches));
        }

        private void DeleteMatch(object? param)
        {
            if (param is MatchRecord m)
            {
                _db.DeleteMatch(m.Id);
                LoadMatches();
            }
        }
    }
}
