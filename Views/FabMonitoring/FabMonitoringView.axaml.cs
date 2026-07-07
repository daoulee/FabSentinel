using Avalonia;
using Avalonia.Controls;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using System.IO;

namespace GreenVision.Views;

public partial class FabMonitoringView : UserControl
{
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;

    public FabMonitoringView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InitializeVideoPlayer();
    }

    private void InitializeVideoPlayer()
    {
        var videoView = this.FindControl<VideoView>("FabVideoView");
        if (videoView is null) return;

        var videoPath = FindVideoFile();
        var placeholder = this.FindControl<Border>("VideoPlaceholder");

        try
        {
            LibVLCSharp.Shared.Core.Initialize();
            _libVLC = new LibVLC(enableDebugLogs: false);
            _mediaPlayer = new MediaPlayer(_libVLC);

            if (videoPath is not null)
            {
                var media = new Media(_libVLC, videoPath, FromType.FromPath);
                media.AddOption(":input-repeat=65535");
                _mediaPlayer.Media = media;
                media.Dispose();

                videoView.MediaPlayer = _mediaPlayer;
                _mediaPlayer.Play();

                if (placeholder is not null)
                    placeholder.IsVisible = false;
            }
            else
            {
                if (placeholder is not null)
                    placeholder.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FabVideoPlayer] VLC init failed: {ex.Message}");
            if (placeholder is not null)
                placeholder.IsVisible = true;
        }
    }

    private static string? FindVideoFile()
    {
        string[] names = ["fabroom.mp4", "fabroom.webm", "fabroom.mov", "fabroom.mkv"];
        foreach (var name in names)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Videos", name);
            if (File.Exists(path)) return path;
        }
        return null;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _mediaPlayer?.Stop();
        _mediaPlayer?.Dispose();
        _libVLC?.Dispose();
        _mediaPlayer = null;
        _libVLC = null;
    }
}
