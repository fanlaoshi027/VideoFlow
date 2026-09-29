namespace VideoFlow.Core.Import;

public sealed class DirectoryRow
{
    public int RowNumber { get; init; }
    public int? PageNumber { get; init; }
    public string PageCode => PageNumber is null ? string.Empty : $"P{PageNumber:00}";
    public string CatalogTitle { get; init; } = string.Empty;
    public string? Chapter { get; init; }
    public string? Section { get; init; }
    public string? Notes { get; init; }
    public string Status { get; set; } = "待检查";
}
