using File = System.IO.File;
using YoutubeDLSharp;

namespace av_tgbot;

public class Downloader
{
    private readonly string _options =
        "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"";
    private readonly float _maxVideoDuration = 180;
    private string? _filePath;

    public async Task<DownloadStatus> Download(string link)
    {
        var ytdl = new YoutubeDL
        {
            YoutubeDLPath = "/usr/local/bin/yt-dlp",
            FFmpegPath = "/usr/local/bin/ffmpeg",
            OutputFolder = "/Users/sergeymelnikov/code/av-tgbot/downloads"
        };

        var dataFetch = await ytdl.RunVideoDataFetch(link);
        var videoDuration= dataFetch.Data.Duration;

        if(videoDuration > _maxVideoDuration)
            return new DownloadStatus(
                false,
                "Video is too long. Should be no more that 3 minutes."
            );

        var result = await ytdl.RunVideoDownload(link, _options);

        if(result.Success)
        {
            _filePath = result.Data;
            return new DownloadStatus(true, "OK");
        }
        
        return new DownloadStatus(
            false,
            $"Error downloading video from link: {link}"
        );
    }

    public Stream GetFileStream()
    {
        return File.Open(_filePath, FileMode.Open);
    }

    public void CleanUp()
    {
        File.Delete(_filePath);
    }
}
