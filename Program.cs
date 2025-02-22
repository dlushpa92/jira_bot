using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using System.Threading;
using Atlassian.Jira;
using its_bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Polling;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using its_bot.Models;
using static Telegram.Bot.TelegramBotClient;


internal class Program
{
    static async Task Main(string[] args)
    {
        var botToken = InfoManager.getToken();

        using var cts = new CancellationTokenSource();
        var bot = new TelegramBotClient(botToken);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
            DropPendingUpdates = true
        };

        var handler = new UpdateHandler();
        handler.OnHandleUpdateStarted += (message) => Console.WriteLine($"Началась обработка сообщения '{message}'");
        handler.OnHandleUpdateCompleted += (message) => Console.WriteLine($"Закончилась обработка сообщения '{message}'");

        bot.StartReceiving(handler, receiverOptions, cts.Token);
        Console.WriteLine("bot запущен!");
        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.A)
                {
                    Console.WriteLine("Завершение работы...");
                    cts.Cancel();
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }
        finally
        {
            handler.OnHandleUpdateStarted -= (message) => Console.WriteLine($"Началась обработка сообщения '{message}'");
            handler.OnHandleUpdateCompleted -= (message) => Console.WriteLine($"Закончилась обработка сообщения '{message}'");
        }
    }
}

