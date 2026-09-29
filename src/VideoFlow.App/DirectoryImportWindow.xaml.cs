using System.Windows;
using Microsoft.Win32;
using VideoFlow.Core.Import;

namespace VideoFlow.App;

public partial class DirectoryImportWindow : Window
{
    private DirectoryImportResult? _result;
    private string? _filePath;

    public DirectoryImportWindow()
    {
        InitializeComponent();
    }

    public DirectoryImportResult? ImportResult => _result;

    private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择教材目录 Excel",
            Filter = "Excel 文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true) return;

        try
        {
            var importer = new ExcelDirectoryImporter();
            _result = importer.Import(dialog.FileName);
            _filePath = dialog.FileName;
            FilePathText.Text = dialog.FileName;
            PreviewGrid.ItemsSource = _result.Rows;
            SummaryText.Text = $"共 {_result.Rows.Count} 条，正常 {_result.ValidCount}，缺页码 {_result.MissingPageCount}，缺目录 {_result.MissingTitleCount}，重复页码 {_result.DuplicatePageCount}";
            ImportButton.IsEnabled = _result.Rows.Count > 0 && _result.Rows.All(x => x.Status == "✓ 可导入");
        }
        catch (Exception ex)
        {
            _result = null;
            ImportButton.IsEnabled = false;
            MessageBox.Show(this, ex.Message, "导入失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_result is null) return;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
