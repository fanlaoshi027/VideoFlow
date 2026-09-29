using Microsoft.Win32;
using VideoFlow.Core.Models;
using VideoFlow.Core.Services;

namespace VideoFlow.App.Services;

public sealed class VideoImportService
{
    private static readonly string[] VideoExtensions = [".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v"];

    public IReadOnlyList<VideoItem> PickVideos()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择录制的视频",
            Multiselect = true,
            Filter = "视频文件|*.mp4;*.mov;*.mkv;*.avi;*.webm;*.m4v|所有文件|*.*"
        };

        if (dialog.ShowDialog() != true)
            return [];

        return dialog.FileNames
            .Where(IsVideo)
            .Select(path => new VideoItem
            {
                SourcePath = path,
                FileName = Path.GetFileName(path),
                PageNumber = PageCodeParser.Parse(path)
            })
            .ToList();
    }

    private static bool IsVideo(string path) => VideoExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
}
