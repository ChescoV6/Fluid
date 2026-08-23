using System.IO;
using System.Text.Json;

namespace Bloxstrap
{
    public class PlaySession
    {
        public string Name { get; set; } = "";
        public long PlaceId { get; set; } = 0;
        public DateTime StartedUtc { get; set; }
        public DateTime? EndedUtc { get; set; }

        [JsonIgnore]
        public TimeSpan Duration => (EndedUtc ?? DateTime.UtcNow) - StartedUtc;
    }

    /// <summary>
    /// Persists play sessions for the stats page.
    /// </summary>
    public class PlayStatsManager : JsonManager<List<PlaySession>>
    {
        public override string ClassName => "PlayStats";

        public static PlayStatsManager Instance { get; } = new();

        private PlaySession? _active;

        public void RecordJoin(long placeId)
        {
            // close any dangling session first
            EndActive();

            _active = new PlaySession
            {
                PlaceId = placeId,
                StartedUtc = DateTime.UtcNow
            };

            Prop.Add(_active);
            Save();
        }

        public void EndActive(string? resolvedName = null)
        {
            if (_active is null)
                return;

            if (!String.IsNullOrEmpty(resolvedName))
                _active.Name = resolvedName;

            _active.EndedUtc = DateTime.UtcNow;
            _active = null;
            Save();
        }

        public (int Sessions, TimeSpan TotalTime, List<(string Name, TimeSpan Time, int Visits)> TopGames) GetSummary()
        {
            var sessions = Prop.Where(x => x.StartedUtc > DateTime.MinValue).ToList();
            var total = TimeSpan.FromTicks(sessions.Sum(s => s.Duration.Ticks));

            var top = sessions.GroupBy(s => String.IsNullOrEmpty(s.Name) ? $"Place {s.PlaceId}" : s.Name)
                              .Select(g => (Name: g.Key, Time: TimeSpan.FromTicks(g.Sum(x => x.Duration.Ticks)), Visits: g.Count()))
                              .OrderByDescending(x => x.Time.Ticks)
                              .Take(10)
                              .ToList();

            return (sessions.Count, total, top);
        }

        public static void ExportCsv(string filePath)
        {
            var lines = new List<string> { "name,place_id,started_utc,ended_utc,duration_minutes" };

            foreach (var session in Instance.Prop.OrderBy(x => x.StartedUtc))
            {
                string name = session.Name.Contains(',') ? $"\"{session.Name}\"" : session.Name;
                lines.Add($"{name},{session.PlaceId},{session.StartedUtc:O},{session.EndedUtc?.ToString("O") ?? ""},{session.Duration.TotalMinutes:F1}");
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}
