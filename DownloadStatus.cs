namespace av_tgbot;

public class DownloadStatus
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }

    public DownloadStatus(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
}
