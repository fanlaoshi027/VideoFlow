using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace VideoFlow.Core.Import;

public sealed class ExcelDirectoryImporter
{
    private static readonly Regex PageRegex = new(@"^\s*[Pp]?\s*(\d+)\s*$", RegexOptions.Compiled);
    private static readonly XNamespace MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public DirectoryImportResult Import(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException("找不到 Excel 文件。", filePath);
        using var zip = ZipFile.OpenRead(filePath);
        var sharedStrings = ReadSharedStrings(zip);
        var workbook = ReadXml(zip, "xl/workbook.xml");
        var rels = ReadXml(zip, "xl/_rels/workbook.xml.rels");
        var firstSheet = workbook.Descendants(MainNs + "sheet").FirstOrDefault() ?? throw new InvalidDataException("Excel 中没有可用的工作表。");
        var relationshipId = firstSheet.Attribute(RelNs + "id")?.Value;
        var relationship = rels.Descendants().FirstOrDefault(x => x.Attribute("Id")?.Value == relationshipId);
        var target = relationship?.Attribute("Target")?.Value ?? "worksheets/sheet1.xml";
        var sheetPath = target.StartsWith("/") ? target.TrimStart('/') : "xl/" + target.TrimStart('/');
        var sheet = ReadXml(zip, sheetPath);
        var rows = sheet.Descendants(MainNs + "row").ToList();
        if (rows.Count == 0) throw new InvalidDataException("Excel 工作表为空。");

        var headers = ReadCells(rows[0], sharedStrings)
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => NormalizeHeader(x.Value), x => x.Column, StringComparer.OrdinalIgnoreCase);
        var pageColumn = FindColumn(headers, "页码", "页面", "page", "p");
        var titleColumn = FindColumn(headers, "目录名称", "目录", "小节名称", "名称", "标题", "title");
        if (pageColumn is null || titleColumn is null) throw new InvalidDataException("Excel 至少需要“页码”和“目录名称”两列。");
        var chapterColumn = FindColumn(headers, "章节", "章", "chapter");
        var sectionColumn = FindColumn(headers, "小节", "节", "section");
        var notesColumn = FindColumn(headers, "备注", "说明", "notes", "remark");

        var result = new DirectoryImportResult();
        foreach (var row in rows.Skip(1))
        {
            var map = ReadCells(row, sharedStrings).ToDictionary(x => x.Column, x => x.Value);
            map.TryGetValue(pageColumn.Value, out var pageText);
            map.TryGetValue(titleColumn.Value, out var title);
            if (string.IsNullOrWhiteSpace(pageText) && string.IsNullOrWhiteSpace(title)) continue;
            map.TryGetValue(chapterColumn ?? -1, out var chapter);
            map.TryGetValue(sectionColumn ?? -1, out var section);
            map.TryGetValue(notesColumn ?? -1, out var notes);
            result.Rows.Add(new DirectoryRow
            {
                RowNumber = int.TryParse(row.Attribute("r")?.Value, out var rowNumber) ? rowNumber : result.Rows.Count + 2,
                PageNumber = ParsePageNumber(pageText), CatalogTitle = title?.Trim() ?? string.Empty,
                Chapter = NullIfEmpty(chapter), Section = NullIfEmpty(section), Notes = NullIfEmpty(notes)
            });
        }
        result.Validate();
        return result;
    }

    private static List<(int Column, string Value)> ReadCells(XElement row, IReadOnlyList<string> sharedStrings)
    {
        var result = new List<(int, string)>();
        foreach (var cell in row.Elements(MainNs + "c"))
        {
            var reference = cell.Attribute("r")?.Value ?? string.Empty;
            var column = ColumnNumber(reference);
            var type = cell.Attribute("t")?.Value;
            var value = cell.Element(MainNs + "v")?.Value ?? string.Empty;
            if (type == "s" && int.TryParse(value, out var index) && index >= 0 && index < sharedStrings.Count) value = sharedStrings[index];
            else if (type == "inlineStr") value = string.Concat(cell.Descendants(MainNs + "t").Select(x => x.Value));
            result.Add((column, value));
        }
        return result;
    }

    private static List<string> ReadSharedStrings(ZipArchive zip)
    {
        var entry = zip.GetEntry("xl/sharedStrings.xml");
        if (entry is null) return [];
        using var stream = entry.Open();
        var xml = XDocument.Load(stream);
        return xml.Descendants(MainNs + "si").Select(si => string.Concat(si.Descendants(MainNs + "t").Select(t => t.Value))).ToList();
    }

    private static XDocument ReadXml(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path.Replace('\\', '/')) ?? throw new InvalidDataException($"Excel 文件缺少必要结构：{path}");
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    private static int ColumnNumber(string reference)
    {
        var number = 0;
        foreach (var ch in reference.TakeWhile(char.IsLetter)) number = number * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
        return number;
    }

    private static int? ParsePageNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var match = PageRegex.Match(value);
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : null;
    }

    private static int? FindColumn(Dictionary<string, int> headers, params string[] names)
    {
        foreach (var name in names) if (headers.TryGetValue(NormalizeHeader(name), out var column)) return column;
        return null;
    }

    private static string NormalizeHeader(string value) => value.Trim().Replace(" ", string.Empty).Replace("　", string.Empty).ToLowerInvariant();
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
