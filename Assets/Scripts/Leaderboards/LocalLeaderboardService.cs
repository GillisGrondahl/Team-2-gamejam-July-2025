using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using VContainer;


public class LeaderboardEntry
{
    public string PlayerName { get; }
    public int Score { get; }
    public int Rank { get; }
    public DateTimeOffset Timestamp { get; }

    public LeaderboardEntry(string playerName, int score, int rank, DateTimeOffset timestamp)
    {
        PlayerName = playerName;
        Score = score;
        Rank = rank;
        Timestamp = timestamp;
    }
}


public class LocalLeaderboardService : ILeaderboardService
{
    [Serializable]
    private class PersistedEntry
    {
        public string PlayerName;
        public int Score;
        public long Ticks; // for DateTimeOffset
    }

    [Serializable]
    private class PersistedData
    {
        public List<PersistedEntry> Entries = new();
    }

    private const string PlayerPrefsKey = "leaderboard_v1";

    private readonly List<LeaderboardEntry> _entries = new();

    [Inject]
    [UnityEngine.Scripting.Preserve]
    public LocalLeaderboardService()
    {
        LoadFromPrefs();
    }

    public Task<IReadOnlyList<LeaderboardEntry>> GetTopEntriesAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        // No real async work here, but keeping Task for future compatibility
        var top = _entries
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.Timestamp)
            .Take(limit)
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<LeaderboardEntry>)top);
    }

    public Task SubmitScoreAsync(
        string playerName,
        int score,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        var now = DateTimeOffset.UtcNow;

        var newEntry = new LeaderboardEntry(
            playerName,
            score,
            rank: 0,    // rank will be recalculated
            timestamp: now);

        _entries.Add(newEntry);
        RecalculateRanks();
        SaveToPrefs();

        return Task.CompletedTask;
    }

    private void RecalculateRanks()
    {
        var ordered = _entries
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.Timestamp)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            var e = ordered[i];
            ordered[i] = new LeaderboardEntry(
                e.PlayerName,
                e.Score,
                rank: i + 1,
                timestamp: e.Timestamp);
        }

        _entries.Clear();
        _entries.AddRange(ordered);
    }

    private void LoadFromPrefs()
    {
        _entries.Clear();

        if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            return;

        var json = PlayerPrefs.GetString(PlayerPrefsKey);
        if (string.IsNullOrEmpty(json))
            return;

        var data = JsonUtility.FromJson<PersistedData>(json);
        if (data == null || data.Entries == null)
            return;

        foreach (var p in data.Entries)
        {
            var dto = new LeaderboardEntry(
                p.PlayerName,
                p.Score,
                rank: 0, // will be recalculated
                timestamp: new DateTimeOffset(p.Ticks, TimeSpan.Zero));
            _entries.Add(dto);
        }

        RecalculateRanks();
    }

    private void SaveToPrefs()
    {
        var data = new PersistedData
        {
            Entries = _entries.Select(e => new PersistedEntry
            {
                PlayerName = e.PlayerName,
                Score = e.Score,
                Ticks = e.Timestamp.UtcTicks
            }).ToList()
        };

        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }
}
