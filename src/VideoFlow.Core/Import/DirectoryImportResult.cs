namespace VideoFlow.Core.Import;

public sealed class DirectoryImportResult
{
    public List<DirectoryRow> Rows { get; } = [];
    public int ValidCount => Rows.Count(x => x.PageNumber is not null && !string.IsNullOrWhiteSpace(x.CatalogTitle));
    public int MissingPageCount => Rows.Count(x => x.PageNumber is null);
    public int MissingTitleCount => Rows.Count(x => x.PageNumber is not null && string.IsNullOrWhiteSpace(x.CatalogTitle));
    public int DuplicatePageCount => Rows
        .Where(x => x.PageNumber is not null)
        .GroupBy(x => x.PageNumber!.Value)
        .Count(g => g.Count() > 1);

    public void Validate()
    {
        var duplicatePages = Rows
            .Where(x => x.PageNumber is not null)
            .GroupBy(x => x.PageNumber!.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        foreach (var row in Rows)
        {
            if (row.PageNumber is null)
                row.Status = "缺少页码";
            else if (string.IsNullOrWhiteSpace(row.CatalogTitle))
                row.Status = "缺少目录名称";
            else if (duplicatePages.Contains(row.PageNumber.Value))
                row.Status = "重复页码";
            else
                row.Status = "✓ 可导入";
        }
    }
}
