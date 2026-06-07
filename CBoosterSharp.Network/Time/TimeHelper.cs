namespace CBoosterSharp.Network.Time;

public static class TimeHelper
{
    /// <summary>
    /// Converts a DateTime or DateTimeOffset to a Unix timestamp (seconds).
    /// Always assumes UTC when ambiguous.
    /// </summary>
    public static long ToUnixTimestamp(DateTime dateTime)
        => new DateTimeOffset(
            DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        ).ToUnixTimeSeconds();

    public static long ToUnixTimestamp(DateTimeOffset dateTimeOffset)
        => dateTimeOffset.ToUnixTimeSeconds();

    /// <summary>
    /// Produces a Laravel-like "diffForHumans" string.
    /// Example: "3 seconds ago", "5 minutes ago", "2 hours ago"
    /// </summary>
    public static string DiffForHumans(DateTimeOffset dateTime)
    {
        var diff = DateTimeOffset.UtcNow - dateTime;

        if (diff.TotalSeconds < 60)
            return $"{(int)diff.TotalSeconds} seconds ago";

        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} minutes ago";

        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} hours ago";

        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays} days ago";

        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)} weeks ago";

        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)} months ago";

        return $"{(int)(diff.TotalDays / 365)} years ago";
    }
}

