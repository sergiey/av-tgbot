using YoutubeDLSharp;
using File = System.IO.File;

namespace av_tgbot;

public class Downloader
{
    private readonly string _options =
        "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"";

    public async Task<Stream> GetFileStream(string link)
    {
        var ytdl = new YoutubeDL
        {
            YoutubeDLPath = "/usr/local/bin/yt-dlp",
            FFmpegPath = "/usr/local/bin/ffmpeg",
            OutputFolder = "/Users/sergeymelnikov/code/av-tgbot/downloads"
        };

        var filePath = await ytdl.RunVideoDownload(link, _options);
        return File.Open(filePath.Data, FileMode.Open);
    }
}
