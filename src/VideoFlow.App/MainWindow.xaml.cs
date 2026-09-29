using System.Collections.ObjectModel;
using System.Windows;
using VideoFlow.App.Services;
using VideoFlow.Core.Import;
using VideoFlow.Core.Models;

namespace VideoFlow.App;

public partial class MainWindow : Window
{
    private readonly VideoImportService _videoImportService = new();
    public ObservableCollection<VideoItem> Videos { get; } = [];
    public ObservableCollection<DirectoryRow> DirectoryRows { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void ImportVideos_Click(object sender, RoutedEventArgs e)
    {
        foreach (var video in _videoImportService.PickVideos())
        {
            if (!Videos.Any(x => string.Equals(x.SourcePath, video.SourcePath, StringComparison.OrdinalIgnoreCase)))
                Videos.Add(video);
        }
        StatusText.Text = $"已导入 {Videos.Count} 个视频";
    }

    private void OpenDirectoryImport_Click(object sender, RoutedEventArgs e)
    {
        var window = new DirectoryImportWindow { Owner = this };
        if (window.ShowDialog() != true || window.ImportResult is null) return;

        DirectoryRows.Clear();
        foreach (var row in window.ImportResult.Rows) DirectoryRows.Add(row);
        StatusText.Text = $"目录已导入：{DirectoryRows.Count} 条，可用于 P 页码匹配";

        foreach (var video in Videos)
        {
            if (video.PageNumber is null) continue;
            var match = DirectoryRows.FirstOrDefault(x => x.PageNumber == video.PageNumber);
            if (match is null) continue;
            video.DirectoryTitle = match.CatalogTitle;
        }
    }
}
