using LoLTracker.Models;
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
                    Notes TEXT
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
        }

        private void CreateTablesIfNotExist() => CreateTables();

        public void InsertMatch(MatchRecord match)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string query = @"INSERT INTO Matches (PlayerName, Champion, IsWin, Date, Notes)
                         VALUES (@playerName, @champion, @isWin, @date, @notes);";
            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerName", string.IsNullOrWhiteSpace(match.PlayerName) ? "Unknown" : match.PlayerName);
            cmd.Parameters.AddWithValue("@champion", match.Champion);
            cmd.Parameters.AddWithValue("@isWin", match.IsWin ? 1 : 0);
            cmd.Parameters.AddWithValue("@date", match.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@notes", (object?)match.Notes ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public List<MatchRecord> GetAllMatches()
        {
            var matches = new List<MatchRecord>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            
            // Check if PlayerName column exists
            bool hasPlayerName = false;
            try
            {
                using var checkCmd = new SQLiteCommand("SELECT PlayerName FROM Matches LIMIT 1;", conn);
                checkCmd.ExecuteScalar();
                hasPlayerName = true;
            }
            catch
            {
                hasPlayerName = false;
            }
            
            string query = hasPlayerName 
                ? "SELECT Id, PlayerName, Champion, IsWin, Date, Notes FROM Matches ORDER BY Date DESC;"
                : "SELECT Id, Champion, IsWin, Date, Notes FROM Matches ORDER BY Date DESC;";
            
            using var cmd = new SQLiteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var m = new MatchRecord
                {
                    Id = reader.GetInt32(0)
                };
                
                if (hasPlayerName)
                {
                    m.PlayerName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1);
                    m.Champion = reader.GetString(2);
                    m.IsWin = reader.GetInt32(3) == 1;
                    m.Date = DateTime.Parse(reader.GetString(4));
                    m.Notes = reader.IsDBNull(5) ? null : reader.GetString(5);
                }
                else
                {
                    m.PlayerName = "Unknown";
                    m.Champion = reader.GetString(1);
                    m.IsWin = reader.GetInt32(2) == 1;
                    m.Date = DateTime.Parse(reader.GetString(3));
                    m.Notes = reader.IsDBNull(4) ? null : reader.GetString(4);
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
    }
}
