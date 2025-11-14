using System;

namespace LoLTracker.Models
{
    public class MatchRecord
    {
        public int Id { get; set; }
        public string Champion { get; set; } = string.Empty;
        public bool IsWin { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
