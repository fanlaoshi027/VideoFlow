using System.Collections.ObjectModel;
using System.Windows;
using VideoFlow.App.Services;
using VideoFlow.Core.Models;

namespace VideoFlow.App;

public partial class MainWindow : Window
{
    private readonly VideoImportService _videoImportService = new();
    public ObservableCollection<VideoItem> Videos { get; } = [];

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
}
