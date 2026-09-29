using System.Text.RegularExpressions;
using VideoFlow.Core.Models;

namespace VideoFlow.Core.Services;

public sealed class VideoNamingService
{
    private static readonly Regex InvalidFileNameChars = new("[<>:\"/\\|?*]", RegexOptions.Compiled);

    public string BuildName(VideoItem video)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(video.PageCode))
            parts.Add(video.PageCode!);

        AddPart(parts, video.Chapter);
        AddPart(parts, video.Section);
        AddPart(parts, video.DirectoryTitle);

        if (parts.Count == 0)
            return video.OriginalFileName;

        var extension = Path.GetExtension(video.OriginalFileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".mp4";

        var baseName = InvalidFileNameChars.Replace(string.Join("_", parts), "_");
        baseName = Regex.Replace(baseName, "_+", "_").Trim(' ', '_', '.');
        return baseName + extension.ToLowerInvariant();
    }

    public void Apply(VideoItem video)
    {
        video.FileName = BuildName(video);
    }

    private static void AddPart(List<string> parts, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            parts.Add(value.Trim());
    }
}
