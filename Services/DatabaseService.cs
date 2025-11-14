using LoLTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

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
                    Champion TEXT NOT NULL,
                    IsWin INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    Notes TEXT
                );";
            using var cmd = new SQLiteCommand(createTable, conn);
            cmd.ExecuteNonQuery();
        }

        private void CreateTablesIfNotExist() => CreateTables();

        public void InsertMatch(MatchRecord match)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string query = @"INSERT INTO Matches (Champion, IsWin, Date, Notes)
                         VALUES (@champion, @isWin, @date, @notes);";
            using var cmd = new SQLiteCommand(query, conn);
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
            string query = "SELECT Id, Champion, IsWin, Date, Notes FROM Matches ORDER BY Date DESC;";
            using var cmd = new SQLiteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var m = new MatchRecord
                {
                    Id = reader.GetInt32(0),
                    Champion = reader.GetString(1),
                    IsWin = reader.GetInt32(2) == 1,
                    Date = DateTime.Parse(reader.GetString(3)),
                    Notes = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
                matches.Add(m);
            }
            return matches;
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
    }
}
