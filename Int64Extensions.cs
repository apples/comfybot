

public static class Int64Extensions
{
    public static DateTimeOffset? ToDateTimeOffset(this long? dt)
    {
        if (dt == null)
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(dt.Value);
    }

    public static string ToEmbedAsTimestamp(this long? dt)
    {
        if (dt == null)
            return "`never`";

        return dt.Value.ToEmbedAsTimestamp();
    }

    public static string ToEmbedAsTimestamp(this long dt)
    {
        return $"<t:{dt}:F>";
    }
}
