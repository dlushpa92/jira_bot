using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using System.Threading;
using Atlassian.Jira;
using its_bot;
using Microsoft.EntityFrameworkCore;
using System;
using Telegram.Bot.Polling;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using its_bot.Models;
using System.Text;
using static Telegram.Bot.TelegramBotClient;


internal class Program
{
    //private static string jiraToken = "";
    //public static Dictionary<long, UserSession> userSessions = new Dictionary<long, UserSession>();
    static async Task Main(string[] args)
    {
        var botToken = InfoManager.getToken();
        ;

        using var cts = new CancellationTokenSource();
        var bot = new TelegramBotClient(botToken);

        //bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: cts.Token);
        //Console.WriteLine("bot start running");
        //Console.ReadLine();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery},
            DropPendingUpdates = true
        };

        var handler = new UpdateHandler();
        handler.OnHandleUpdateStarted += (message) => Console.WriteLine($"Началась обработка сообщения '{message}'");
        handler.OnHandleUpdateCompleted += (message) => Console.WriteLine($"Закончилась обработка сообщения '{message}'");

        bot.StartReceiving(handler, receiverOptions, cts.Token);
        ;
        //var me = bot.GetMe();
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

