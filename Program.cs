using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using its_bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Polling;
using Microsoft.Extensions.Configuration;
using Serilog;

internal class Program
{
    //private static string jiraToken = "";
    //public static Dictionary<long, UserSession> userSessions = new Dictionary<long, UserSession>();
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string botToken = configuration["TelegramBot:BotToken"];
        string connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(botToken))
        {
            Console.WriteLine("Не настроен токена бота. Проверьте конфигурацию проекта.");
            return;
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Не настроено подключение к БД.Проверьте конфигурацию проекта.");
            return;
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Логи в файл
            .CreateLogger();

        using var cts = new CancellationTokenSource();
        var bot = new TelegramBotClient(botToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
            DropPendingUpdates = true
        };

        var handler = new UpdateHandler(configuration);
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

