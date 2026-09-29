namespace VideoFlow.Core.Models;

public sealed class TextbookPage
{
    public string TextbookId { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public string PageCode => $"P{PageNumber:00}";
    public string CatalogTitle { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public string? Section { get; set; }
    public string? Notes { get; set; }
}
