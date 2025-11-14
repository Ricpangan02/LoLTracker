using LoLTracker.Services;
using System;
using System.Windows.Input;

namespace LoLTracker.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private readonly Action _onSaved;

        public string SummonerName { get; set; } = string.Empty;
        public ICommand ClearAllCommand { get; }
        public ICommand SaveCommand { get; }

        public SettingsViewModel(DatabaseService db, Action? onSaved = null)
        {
            _db = db;
            _onSaved = onSaved ?? (() => { });
            ClearAllCommand = new RelayCommand(ClearAll);
            SaveCommand = new RelayCommand(() => _onSaved());
        }

        private void ClearAll()
        {
            _db.ClearAll();
        }
    }
}
