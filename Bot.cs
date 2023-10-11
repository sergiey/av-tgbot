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

    public Bot(string token, CancellationToken cancelToken)
    {
        _token = token;
        _cancelToken = cancelToken;
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
                "Send a video link");
            return;
        }

        var chatId = message.Chat.Id;
        Console.WriteLine(
            $"Received a '{messageText}' message in chat {chatId}.");

        try
        {
            var downloader = new Downloader();
            var status = downloader.Download(messageText).Result;

            if(status.IsSuccess)
            {
                await botClient.SendVideoAsync(
                    chatId: chatId,
                    video: InputFile.FromStream(downloader.GetFileStream()),
                    cancellationToken: cancellationToken);
                downloader.CleanUp();
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: status.Message,
                    cancellationToken: cancellationToken);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Incorrect link.",
                cancellationToken: cancellationToken);
        }
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
