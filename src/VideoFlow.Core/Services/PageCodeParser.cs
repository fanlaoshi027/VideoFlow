using System.Text.RegularExpressions;

namespace VideoFlow.Core.Services;

public static class PageCodeParser
{
    private static readonly Regex Pattern = new(@"(?<![A-Za-z0-9])P(?<number>\d{1,4})(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static int? Parse(string fileName)
    {
        var match = Pattern.Match(Path.GetFileNameWithoutExtension(fileName));
        return match.Success && int.TryParse(match.Groups["number"].Value, out var number)
            ? number
            : null;
    }
}
