

public static class DateTimeOffsetExtensions
{
    public static string ToEmbed(this DateTimeOffset? dt)
    {
        if (dt == null)
            return "`never`";

        return $"<t:{dt.Value.ToUnixTimeSeconds()}:F>";
    }
}
