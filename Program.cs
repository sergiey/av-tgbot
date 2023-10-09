using Microsoft.Extensions.Configuration;
using av_tgbot;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var token = config["Token"];

using CancellationTokenSource cts = new();
var bot = new Bot(token, cts.Token);

await bot.Run();
while(true) {
    string? o = Console.ReadLine();
    if (o?.ToLower() == "exit" || o?.ToLower() == "quit")
    {
        cts.Cancel();
        return;
    }
}
