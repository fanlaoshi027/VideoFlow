using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace VideoFlow.Core.Import;

public sealed class ExcelDirectoryImporter
{
    private static readonly Regex PageRegex = new(@"^\s*[Pp]?\s*(\d+)\s*$", RegexOptions.Compiled);

    public DirectoryImportResult Import(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("找不到 Excel 文件。", filePath);

        var result = new DirectoryImportResult();
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidDataException("Excel 中没有可用的工作表。 ");

        var used = worksheet.RangeUsed()
            ?? throw new InvalidDataException("Excel 工作表为空。 ");

        var headerRow = used.FirstRowUsed();
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.Cells())
        {
            var header = NormalizeHeader(cell.GetString());
            if (!string.IsNullOrWhiteSpace(header))
                headers[header] = cell.Address.ColumnNumber;
        }

        var pageColumn = FindColumn(headers, "页码", "页面", "page", "p");
        var titleColumn = FindColumn(headers, "目录名称", "目录", "小节名称", "名称", "标题", "title");
        if (pageColumn is null || titleColumn is null)
            throw new InvalidDataException("Excel 至少需要“页码”和“目录名称”两列。 ");

        var chapterColumn = FindColumn(headers, "章节", "章", "chapter");
        var sectionColumn = FindColumn(headers, "小节", "节", "section");
        var notesColumn = FindColumn(headers, "备注", "说明", "notes", "remark");

        foreach (var row in used.RowsUsed().Skip(1))
        {
            var pageText = row.Cell(pageColumn.Value).GetString().Trim();
            var title = row.Cell(titleColumn.Value).GetString().Trim();
            var chapter = chapterColumn is null ? null : NullIfEmpty(row.Cell(chapterColumn.Value).GetString());
            var section = sectionColumn is null ? null : NullIfEmpty(row.Cell(sectionColumn.Value).GetString());
            var notes = notesColumn is null ? null : NullIfEmpty(row.Cell(notesColumn.Value).GetString());

            if (string.IsNullOrWhiteSpace(pageText) && string.IsNullOrWhiteSpace(title))
                continue;

            result.Rows.Add(new DirectoryRow
            {
                RowNumber = row.RowNumber(),
                PageNumber = ParsePageNumber(pageText),
                CatalogTitle = title,
                Chapter = chapter,
                Section = section,
                Notes = notes
            });
        }

        result.Validate();
        return result;
    }

    private static int? ParsePageNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var match = PageRegex.Match(value);
        if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            return number;
        return null;
    }

    private static int? FindColumn(Dictionary<string, int> headers, params string[] names)
    {
        foreach (var name in names)
        {
            var normalized = NormalizeHeader(name);
            if (headers.TryGetValue(normalized, out var column)) return column;
        }
        return null;
    }

    private static string NormalizeHeader(string value) =>
        value.Trim().Replace(" ", string.Empty).Replace("　", string.Empty).ToLowerInvariant();

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
