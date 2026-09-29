namespace VideoFlow.Core.Models;

public sealed class VideoItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string SourcePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
    public string? PageCode => PageNumber is null ? null : $"P{PageNumber}";
    public string? TextbookId { get; set; }
    public string? DirectoryTitle { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? FrameRate { get; set; }
    public DateTime ImportedAt { get; init; } = DateTime.Now;
}
