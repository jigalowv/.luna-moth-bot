namespace Luna.Presentation.Extensions;

public static class StringExtensions
{
    public static List<ulong> ParseToUlongList(this string targets)
    {
        if (string.IsNullOrWhiteSpace(targets))
        {
            return new List<ulong>();
        }

        return targets
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(idStr => ulong.TryParse(idStr, out var id) ? id : (ulong?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }
}