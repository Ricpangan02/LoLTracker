using LoLTracker.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace LoLTracker.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath = Path.Combine("Data", "database.db");
        private readonly string _connectionString;

        public DatabaseService()
        {
            Directory.CreateDirectory("Data");
            _connectionString = $"Data Source={_dbPath};Version=3;";

            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
                CreateTables();
            }
            else
            {
                // ensure tables exist (safe)
                CreateTablesIfNotExist();
            }
        }

        private void CreateTables()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string createTable = @"
                CREATE TABLE IF NOT EXISTS Matches (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerName TEXT NOT NULL DEFAULT 'Unknown',
                    Champion TEXT NOT NULL,
                    IsWin INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    GameMode TEXT NOT NULL DEFAULT 'Selected'
                );";
            using var cmd = new SQLiteCommand(createTable, conn);
            cmd.ExecuteNonQuery();
            
            // Create Settings table
            string createSettingsTable = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                );";
            using var settingsCmd = new SQLiteCommand(createSettingsTable, conn);
            settingsCmd.ExecuteNonQuery();
            
            // Create Players table
            string createPlayersTable = @"
                CREATE TABLE IF NOT EXISTS Players (
                    PlayerName TEXT PRIMARY KEY
                );";
            using var playersCmd = new SQLiteCommand(createPlayersTable, conn);
            playersCmd.ExecuteNonQuery();
            
            // Migrate existing database - add PlayerName column if it doesn't exist
            try
            {
                string alterTable = "ALTER TABLE Matches ADD COLUMN PlayerName TEXT NOT NULL DEFAULT 'Unknown';";
                using var alterCmd = new SQLiteCommand(alterTable, conn);
                alterCmd.ExecuteNonQuery();
            }
            catch
            {
                // Column already exists, ignore
            }
            
            // Migrate Notes to GameMode if Notes exists but GameMode doesn't
            try
            {
                using var checkCmd = new SQLiteCommand("SELECT GameMode FROM Matches LIMIT 1;", conn);
                checkCmd.ExecuteScalar();
            }
            catch
            {
                // GameMode column doesn't exist, migrate from Notes
                try
                {
                    string alterToGameMode = "ALTER TABLE Matches ADD COLUMN GameMode TEXT NOT NULL DEFAULT 'Selected';";
                    using var alterCmd = new SQLiteCommand(alterToGameMode, conn);
                    alterCmd.ExecuteNonQuery();
                    
                    // Copy Notes to GameMode if Notes exists
                    try
                    {
                        string updateGameMode = "UPDATE Matches SET GameMode = COALESCE(Notes, 'Selected') WHERE GameMode = 'Selected';";
                        using var updateCmd = new SQLiteCommand(updateGameMode, conn);
                        updateCmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Notes column might not exist, ignore
                    }
                }
                catch
                {
                    // Migration failed, ignore
                }
            }
        }

        private void CreateTablesIfNotExist() => CreateTables();

        public void InsertMatch(MatchRecord match)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string query = @"INSERT INTO Matches (PlayerName, Champion, IsWin, Date, GameMode)
                         VALUES (@playerName, @champion, @isWin, @date, @gameMode);";
            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerName", string.IsNullOrWhiteSpace(match.PlayerName) ? "Unknown" : match.PlayerName);
            cmd.Parameters.AddWithValue("@champion", match.Champion);
            cmd.Parameters.AddWithValue("@isWin", match.IsWin ? 1 : 0);
            cmd.Parameters.AddWithValue("@date", match.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@gameMode", string.IsNullOrWhiteSpace(match.GameMode) ? "Selected" : match.GameMode);
            cmd.ExecuteNonQuery();
        }

        public List<MatchRecord> GetAllMatches()
        {
            var matches = new List<MatchRecord>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            
            // Check which columns exist for backward compatibility
            bool hasPlayerName = false;
            bool hasGameMode = false;
            bool hasNotes = false;
            
            try
            {
                using var checkCmd = new SQLiteCommand("SELECT PlayerName FROM Matches LIMIT 1;", conn);
                checkCmd.ExecuteScalar();
                hasPlayerName = true;
            }
            catch { }
            
            try
            {
                using var checkCmd = new SQLiteCommand("SELECT GameMode FROM Matches LIMIT 1;", conn);
                checkCmd.ExecuteScalar();
                hasGameMode = true;
            }
            catch { }
            
            try
            {
                using var checkCmd = new SQLiteCommand("SELECT Notes FROM Matches LIMIT 1;", conn);
                checkCmd.ExecuteScalar();
                hasNotes = true;
            }
            catch { }
            
            // Build query based on available columns
            string query;
            if (hasPlayerName && hasGameMode)
            {
                query = "SELECT Id, PlayerName, Champion, IsWin, Date, GameMode FROM Matches ORDER BY Date DESC;";
            }
            else if (hasPlayerName && hasNotes)
            {
                query = "SELECT Id, PlayerName, Champion, IsWin, Date, Notes FROM Matches ORDER BY Date DESC;";
            }
            else if (hasPlayerName)
            {
                query = "SELECT Id, PlayerName, Champion, IsWin, Date FROM Matches ORDER BY Date DESC;";
            }
            else if (hasNotes)
            {
                query = "SELECT Id, Champion, IsWin, Date, Notes FROM Matches ORDER BY Date DESC;";
            }
            else
            {
                query = "SELECT Id, Champion, IsWin, Date FROM Matches ORDER BY Date DESC;";
            }
            
            using var cmd = new SQLiteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var m = new MatchRecord
                {
                    Id = reader.GetInt32(0)
                };
                
                int colIndex = 1;
                
                if (hasPlayerName)
                {
                    m.PlayerName = reader.IsDBNull(colIndex) ? "Unknown" : reader.GetString(colIndex);
                    colIndex++;
                }
                else
                {
                    m.PlayerName = "Unknown";
                }
                
                m.Champion = reader.GetString(colIndex++);
                m.IsWin = reader.GetInt32(colIndex++) == 1;
                m.Date = DateTime.Parse(reader.GetString(colIndex++));
                
                // Handle GameMode/Notes
                if (colIndex < reader.FieldCount)
                {
                    if (hasGameMode)
                    {
                        m.GameMode = reader.IsDBNull(colIndex) ? "Selected" : reader.GetString(colIndex);
                    }
                    else if (hasNotes)
                    {
                        // Migrate Notes to GameMode
                        var notesValue = reader.IsDBNull(colIndex) ? null : reader.GetString(colIndex);
                        m.GameMode = string.IsNullOrWhiteSpace(notesValue) ? "Selected" : notesValue;
                    }
                    else
                    {
                        m.GameMode = "Selected";
                    }
                }
                else
                {
                    m.GameMode = "Selected";
                }
                
                matches.Add(m);
            }
            return matches;
        }
        
        public List<string> GetAllPlayerNames()
        {
            var players = new List<string>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            
            // Get players from Matches table
            try
            {
                string query = "SELECT DISTINCT PlayerName FROM Matches WHERE PlayerName IS NOT NULL AND PlayerName != '';";
                using var cmd = new SQLiteCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var player = reader.GetString(0);
                    if (!players.Contains(player))
                    {
                        players.Add(player);
                    }
                }
            }
            catch
            {
                // Column doesn't exist yet
            }
            
            // Get players from Players table
            try
            {
                string query = "SELECT PlayerName FROM Players;";
                using var cmd = new SQLiteCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var player = reader.GetString(0);
                    if (!players.Contains(player))
                    {
                        players.Add(player);
                    }
                }
            }
            catch
            {
                // Table doesn't exist yet
            }
            
            return players.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public void AddPlayer(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return;
            }

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            
            try
            {
                string query = "INSERT OR IGNORE INTO Players (PlayerName) VALUES (@playerName);";
                using var cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@playerName", playerName.Trim());
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Table doesn't exist yet, ignore
            }
        }

        public void DeleteMatch(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string q = "DELETE FROM Matches WHERE Id = @id;";
            using var cmd = new SQLiteCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void ClearAll()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string q = "DELETE FROM Matches;";
            using var cmd = new SQLiteCommand(q, conn);
            cmd.ExecuteNonQuery();
        }

        public void SaveSetting(string key, string value)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string query = @"
                INSERT OR REPLACE INTO Settings (Key, Value)
                VALUES (@key, @value);";
            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.ExecuteNonQuery();
        }

        public string? GetSetting(string key)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string query = "SELECT Value FROM Settings WHERE Key = @key;";
            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@key", key);
            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }

        public (int imported, int failed, List<string> errors) ImportMatchesFromExcel(string filePath)
        {
            var imported = 0;
            var failed = 0;
            var errors = new List<string>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0]; // Get first worksheet

                if (worksheet == null || worksheet.Dimension == null)
                {
                    errors.Add("Worksheet is empty or invalid");
                    return (0, 0, errors);
                }

                var startRow = 1;
                var endRow = worksheet.Dimension.End.Row;

                // Check if first row is a header
                var firstRowValue = worksheet.Cells[1, 1].Text?.ToLower() ?? "";
                var hasHeader = firstRowValue.Contains("playername") || 
                               firstRowValue.Contains("champion") || 
                               firstRowValue.Contains("iswin") ||
                               firstRowValue.Contains("date");

                int playerNameCol, championCol, isWinCol, dateCol, gameModeCol;

                if (hasHeader)
                {
                    startRow = 2;
                    // Find column indices by header names (flexible column order)
                    playerNameCol = FindColumnIndex(worksheet, 1, "playername", "player");
                    championCol = FindColumnIndex(worksheet, 1, "champion", "champ");
                    isWinCol = FindColumnIndex(worksheet, 1, "iswin", "win", "victory", "result");
                    dateCol = FindColumnIndex(worksheet, 1, "date", "datetime", "time");
                    gameModeCol = FindColumnIndex(worksheet, 1, "gamemode", "game mode", "mode", "notes", "note");

                    // Validate required columns
                    if (championCol == -1)
                    {
                        errors.Add("Champion column not found. Expected column header: 'Champion' or 'Champ'");
                        return (0, 0, errors);
                    }
                    if (isWinCol == -1)
                    {
                        errors.Add("Win/Loss column not found. Expected column header: 'IsWin', 'Win', 'Victory', or 'Result'");
                        return (0, 0, errors);
                    }
                    if (dateCol == -1)
                    {
                        errors.Add("Date column not found. Expected column header: 'Date', 'DateTime', or 'Time'");
                        return (0, 0, errors);
                    }
                }
                else
                {
                    // No header - assume standard column order: PlayerName, Champion, IsWin, Date, GameMode
                    playerNameCol = 1;
                    championCol = 2;
                    isWinCol = 3;
                    dateCol = 4;
                    gameModeCol = 5;
                }

                for (int row = startRow; row <= endRow; row++)
                {
                    try
                    {
                        var champion = worksheet.Cells[row, championCol].Text?.Trim();
                        if (string.IsNullOrWhiteSpace(champion))
                        {
                            failed++;
                            errors.Add($"Row {row}: Champion name is required");
                            continue;
                        }

                        var isWinValue = worksheet.Cells[row, isWinCol].Text?.Trim() ?? "";
                        var dateValue = worksheet.Cells[row, dateCol].Text?.Trim() ?? "";
                        var playerName = playerNameCol > 0 ? (worksheet.Cells[row, playerNameCol].Text?.Trim() ?? "") : "Unknown";
                        var gameMode = gameModeCol > 0 ? (worksheet.Cells[row, gameModeCol].Text?.Trim() ?? "") : "Selected";

                        var match = new MatchRecord
                        {
                            PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Unknown" : playerName,
                            Champion = champion,
                            IsWin = ParseBool(isWinValue),
                            Date = ParseDate(dateValue),
                            GameMode = string.IsNullOrWhiteSpace(gameMode) ? "Selected" : gameMode
                        };

                        InsertMatch(match);
                        imported++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        errors.Add($"Row {row}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error reading Excel file: {ex.Message}");
            }

            return (imported, failed, errors);
        }

        private int FindColumnIndex(ExcelWorksheet worksheet, int headerRow, params string[] searchTerms)
        {
            if (worksheet.Dimension == null) return -1;

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cellValue = worksheet.Cells[headerRow, col].Text?.ToLower().Trim() ?? "";
                foreach (var term in searchTerms)
                {
                    if (cellValue.Contains(term.ToLower()))
                    {
                        return col;
                    }
                }
            }
            return -1;
        }

        private bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.ToLower().Trim();
            return value == "true" || value == "1" || value == "yes" || value == "win" || value == "victory";
        }

        private DateTime ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DateTime.Now;

            // Try various date formats
            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    return date;
                }
            }

            // Fallback to standard parsing
            if (DateTime.TryParse(value, out var parsedDate))
            {
                return parsedDate;
            }

            return DateTime.Now;
        }
    }
}
