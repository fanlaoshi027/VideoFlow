using System.Text.RegularExpressions;
using VideoFlow.Core.Models;

namespace VideoFlow.Core.Services;

public sealed class VideoPageMatcher
{
    private static readonly Regex PageRegex = new(@"(?:^|[^A-Za-z0-9])[Pp]\s*(\d+)", RegexOptions.Compiled);

    public int? ExtractPageNumber(string fileName)
    {
        var match = PageRegex.Match(Path.GetFileNameWithoutExtension(fileName));
        return match.Success && int.TryParse(match.Groups[1].Value, out var page) ? page : null;
    }

    public void Match(VideoItem video, IEnumerable<TextbookPage> pages)
    {
        if (video.PageNumber is null)
            video.PageNumber = ExtractPageNumber(video.OriginalFileName);

        if (video.PageNumber is null)
            return;

        var page = pages.FirstOrDefault(x => x.PageNumber == video.PageNumber.Value);
        if (page is null)
            return;

        video.TextbookId = page.TextbookId;
        video.Chapter = page.Chapter;
        video.Section = page.Section;
        video.DirectoryTitle = page.CatalogTitle;
    }
}
