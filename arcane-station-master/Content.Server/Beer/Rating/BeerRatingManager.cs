using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Robust.Shared.ContentPack;

namespace Content.Server.Beer.Rating;

public sealed class BeerRatingManager
{
    [Dependency] private readonly IResourceManager _res = default!;

    public static readonly TimeSpan VoteCooldown = TimeSpan.FromMinutes(30);

    private SqliteConnection? _conn;
    private readonly object _lock = new();

    public void Initialize()
    {
        SqliteConnection getConn()
        {
            if (_res.UserData.RootDir == null)
                return new SqliteConnection("Data Source=:memory:");
            var path = Path.Combine(_res.UserData.RootDir, "beer_rating.db");
            return new SqliteConnection($"Data Source={path}");
        }

        _conn = getConn();
        _conn.Open();

        using var cmd = _conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS beer_rating_score (
    username TEXT PRIMARY KEY COLLATE NOCASE,
    score INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS beer_rating_vote (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    voter TEXT NOT NULL COLLATE NOCASE,
    target TEXT NOT NULL COLLATE NOCASE,
    tag TEXT NOT NULL,
    is_positive INTEGER NOT NULL,
    created_at TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_beer_rating_vote_voter ON beer_rating_vote(voter);
CREATE INDEX IF NOT EXISTS idx_beer_rating_vote_target ON beer_rating_vote(target);
";
        cmd.ExecuteNonQuery();
    }

    public int GetScore(string username)
    {
        if (_conn == null) return 0;
        lock (_lock)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT score FROM beer_rating_score WHERE username = $u";
            cmd.Parameters.AddWithValue("$u", username);
            var result = cmd.ExecuteScalar();
            return result is long l ? (int)l : 0;
        }
    }

    public TimeSpan TimeUntilNextVote(string voter, DateTime now)
    {
        if (_conn == null) return TimeSpan.Zero;
        lock (_lock)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT created_at FROM beer_rating_vote WHERE voter = $v ORDER BY id DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$v", voter);
            var result = cmd.ExecuteScalar();
            if (result is not string s || !DateTime.TryParse(s, out var last))
                return TimeSpan.Zero;
            var elapsed = now - last.ToUniversalTime();
            var remaining = VoteCooldown - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    public bool TryCastVote(string voter, string target, string tag, bool isPositive, out TimeSpan cooldownLeft)
    {
        cooldownLeft = TimeSpan.Zero;
        if (_conn == null) return false;
        if (string.Equals(voter, target, StringComparison.OrdinalIgnoreCase)) return false;

        lock (_lock)
        {
            var now = DateTime.UtcNow;
            cooldownLeft = TimeUntilNextVote(voter, now);
            if (cooldownLeft > TimeSpan.Zero)
                return false;

            using var tx = _conn.BeginTransaction();

            using (var insert = _conn.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO beer_rating_vote (voter, target, tag, is_positive, created_at) VALUES ($v, $t, $tag, $p, $at)";
                insert.Parameters.AddWithValue("$v", voter);
                insert.Parameters.AddWithValue("$t", target);
                insert.Parameters.AddWithValue("$tag", tag);
                insert.Parameters.AddWithValue("$p", isPositive ? 1 : 0);
                insert.Parameters.AddWithValue("$at", now.ToString("o"));
                insert.ExecuteNonQuery();
            }

            var delta = isPositive ? 1 : -1;
            using (var upsert = _conn.CreateCommand())
            {
                upsert.Transaction = tx;
                upsert.CommandText = @"
INSERT INTO beer_rating_score (username, score) VALUES ($u, $d)
ON CONFLICT(username) DO UPDATE SET score = score + $d";
                upsert.Parameters.AddWithValue("$u", target);
                upsert.Parameters.AddWithValue("$d", delta);
                upsert.ExecuteNonQuery();
            }

            tx.Commit();
            return true;
        }
    }

    public void SetScore(string username, int score)
    {
        if (_conn == null) return;
        lock (_lock)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO beer_rating_score (username, score) VALUES ($u, $s)
ON CONFLICT(username) DO UPDATE SET score = $s";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$s", score);
            cmd.ExecuteNonQuery();
        }
    }

    public int AdjustScore(string username, int delta)
    {
        if (_conn == null) return 0;
        lock (_lock)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO beer_rating_score (username, score) VALUES ($u, $d)
ON CONFLICT(username) DO UPDATE SET score = score + $d
RETURNING score";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$d", delta);
            var result = cmd.ExecuteScalar();
            return result is long l ? (int)l : delta;
        }
    }

    public void CastVoteAdmin(string voter, string target, string tag, bool isPositive)
    {
        if (_conn == null) return;
        lock (_lock)
        {
            using var tx = _conn.BeginTransaction();

            using (var insert = _conn.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO beer_rating_vote (voter, target, tag, is_positive, created_at) VALUES ($v, $t, $tag, $p, $at)";
                insert.Parameters.AddWithValue("$v", voter);
                insert.Parameters.AddWithValue("$t", target);
                insert.Parameters.AddWithValue("$tag", tag);
                insert.Parameters.AddWithValue("$p", isPositive ? 1 : 0);
                insert.Parameters.AddWithValue("$at", DateTime.UtcNow.ToString("o"));
                insert.ExecuteNonQuery();
            }

            var delta = isPositive ? 1 : -1;
            using (var upsert = _conn.CreateCommand())
            {
                upsert.Transaction = tx;
                upsert.CommandText = @"
INSERT INTO beer_rating_score (username, score) VALUES ($u, $d)
ON CONFLICT(username) DO UPDATE SET score = score + $d";
                upsert.Parameters.AddWithValue("$u", target);
                upsert.Parameters.AddWithValue("$d", delta);
                upsert.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }

    public List<(string Voter, string Target, string Tag, bool IsPositive, DateTime CreatedAt)> GetLog(string? filter, int limit = 50)
    {
        var rows = new List<(string, string, string, bool, DateTime)>();
        if (_conn == null) return rows;
        lock (_lock)
        {
            using var cmd = _conn.CreateCommand();
            if (string.IsNullOrEmpty(filter))
            {
                cmd.CommandText = "SELECT voter, target, tag, is_positive, created_at FROM beer_rating_vote ORDER BY id DESC LIMIT $lim";
            }
            else
            {
                cmd.CommandText = "SELECT voter, target, tag, is_positive, created_at FROM beer_rating_vote WHERE voter = $f OR target = $f ORDER BY id DESC LIMIT $lim";
                cmd.Parameters.AddWithValue("$f", filter);
            }
            cmd.Parameters.AddWithValue("$lim", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var voter = reader.GetString(0);
                var target = reader.GetString(1);
                var tag = reader.GetString(2);
                var isPositive = reader.GetInt32(3) != 0;
                var createdAt = DateTime.Parse(reader.GetString(4));
                rows.Add((voter, target, tag, isPositive, createdAt));
            }
        }
        return rows;
    }

    public static string ScoreLabel(int score)
    {
        return score switch
        {
            <= -10 => "презренный",
            <= -3 => "ненадёжный",
            <= 10 => "признанный",
            _ => "уважаемый",
        };
    }
}
