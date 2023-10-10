using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace av_tgbot;

public class Bot
{
    private readonly string _token;
    private readonly CancellationToken _cancelToken;
    private readonly Downloader _downloader;
    // private FileStream stream = new();

    public Bot(string token, CancellationToken cancelToken)
    {
        _token = token;
        _cancelToken = cancelToken;
        _downloader = new Downloader();
    }

    public async Task Run()
    {
        TelegramBotClient botClient = new TelegramBotClient(_token);

        ReceiverOptions receiverOptions = new() {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cancelToken
        );

        var me = await botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
    }

    async Task HandleUpdateAsync(
        ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if(update.Message is not { } message)
            return;
        if(message.Text is not { } messageText)
            return;
        if(message.Text.ToLower() == "/start"){
            await botClient.SendTextMessageAsync(
                message.Chat,
                "Отправьте ссылку");
            return;
        }

        var chatId = message.Chat.Id;
        Console.WriteLine(
            $"Received a '{messageText}' message in chat {chatId}.");

        Message sentMessage = await botClient.SendVideoAsync(
            chatId: chatId,
            video: InputFile.FromStream(await _downloader.GetFileStream(messageText)),
            cancellationToken: cancellationToken);
    }

    Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n" +
                    $"{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
