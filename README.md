# LoLTracker

A Windows desktop application for tracking League of Legends Arena mode matches, statistics, and champion performance. Built with WPF and .NET 8.0.

## Features

### 📊 Dashboard
- Overview of match statistics
- Win/loss tracking
- Player performance metrics
- Quick access to all features

### 📝 Log Matches
- Record new Arena matches
- Select champion from complete champion list (171 champions)
- Track win/loss results
- Assign matches to different players
- Set game mode/notes for each match

### 📜 Match History
- View all recorded matches in chronological order
- Delete individual matches
- Filter and search through match records

### 🏆 Champion Statistics
- View win rates for each champion
- Track wins and losses per champion
- Identify your best and worst performing champions

### 👥 Multi-Player Support
- Track matches for multiple players
- Individual player statistics
- Best/worst champion tracking per player

### ⚙️ Settings
- Configure summoner name
- Manage player list
- Import matches from Excel files
- Export data functionality

### 📥 Excel Import
- Import match data from Excel spreadsheets
- Flexible column detection (supports various header formats)
- Automatic date parsing
- Error reporting for failed imports

## Requirements

- **.NET 8.0 Runtime** (Windows)
- **Windows 10/11**
- SQLite (included with the application)

## Installation

1. Download the latest release from the [Releases](../../releases) page
2. Extract the files to your desired location
3. Run `LoLTracker.exe`

The application will automatically create a `Data` folder with a SQLite database on first launch.

## Usage

### Logging a Match

1. Navigate to **Log Match** from the main menu
2. Select or enter a player name
3. Choose the champion you played
4. Select Win or Loss
5. Optionally set the game mode
6. Click **Log Match**

### Viewing Statistics

- **Dashboard**: See overall statistics and quick overview
- **Match History**: Browse all recorded matches
- **Champions**: View champion-specific win rates and performance

### Importing from Excel

1. Go to **Settings**
2. Click **Import from Excel**
3. Select your Excel file (.xlsx)
4. The application will automatically detect columns and import matches

**Excel Format:**
- Required columns: `Champion`, `IsWin` (or `Win`/`Victory`), `Date`
- Optional columns: `PlayerName`, `GameMode` (or `Notes`)
- First row can be headers (automatically detected)
- Supports various date formats

### Managing Players

- Players are automatically added when you log matches
- Manage players in the **Settings** section
- Player names are case-insensitive and sorted alphabetically

## Project Structure

```
LoLTracker/
├── Assets/              # Application icons and resources
├── Converters/          # WPF value converters
├── Data/               # SQLite database (created at runtime)
├── Models/              # Data models
│   ├── ChampionStats.cs
│   ├── MatchRecord.cs
│   └── PlayerStats.cs
├── Services/           # Business logic services
│   ├── ChampionService.cs
│   └── DatabaseService.cs
├── ViewModels/         # MVVM view models
│   ├── BaseViewModel.cs
│   ├── ChampionListModel.cs
│   ├── DashboardViewModel.cs
│   ├── LogMatchViewModel.cs
│   ├── MainViewModel.cs
│   ├── MatchHistoryViewModel.cs
│   ├── RelayCommand.cs
│   └── SettingsViewModel.cs
└── Views/              # WPF XAML views
    ├── ChampionListView.xaml
    ├── DashBoardView.xaml
    ├── LogMatchView.xaml
    ├── MatchHistoryView.xaml
    └── SettingsView.xaml
```

## Technologies Used

- **.NET 8.0** - Framework
- **WPF (Windows Presentation Foundation)** - UI framework
- **SQLite** - Local database storage
- **EPPlus** - Excel file handling
- **MVVM Pattern** - Architecture pattern

## Database Schema

The application uses SQLite with the following tables:

### Matches
- `Id` (INTEGER PRIMARY KEY)
- `PlayerName` (TEXT)
- `Champion` (TEXT)
- `IsWin` (INTEGER)
- `Date` (TEXT)
- `GameMode` (TEXT)

### Settings
- `Key` (TEXT PRIMARY KEY)
- `Value` (TEXT)

### Players
- `PlayerName` (TEXT PRIMARY KEY)

## Building from Source

1. Clone the repository
2. Open `LoLTracker.csproj` in Visual Studio 2022 or later
3. Restore NuGet packages
4. Build and run (F5)

### Dependencies

- EPPlus 7.5.2
- System.Data.SQLite.Core 1.0.118

## Notes

- The database is stored locally in the `Data` folder
- All data is stored on your machine (no cloud sync)
- The application is designed specifically for Arena mode tracking
- Champion list includes all 171 champions as of 2025

## License

This project is provided as-is for personal use.

## Contributing

Feel free to submit issues or pull requests for improvements!
