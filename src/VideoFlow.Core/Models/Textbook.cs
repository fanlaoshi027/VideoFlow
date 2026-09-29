namespace VideoFlow.Core.Models;

public sealed class Textbook
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Edition { get; set; }
    public string? Description { get; set; }
    public string CommonDescription { get; set; } = string.Empty;
    public List<string> CommonTags { get; set; } = [];
    public List<TextbookPage> Pages { get; set; } = [];
}

public sealed class TextbookPage
{
    public int PageNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public string? Section { get; set; }
}
