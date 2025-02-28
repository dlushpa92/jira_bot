using its_bot.Data;
using its_bot.Models;
using its_bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Serilog;

namespace its_bot
{
    public class UserManager
    {
        public static async Task<string> GetUserFromDB(ITelegramBotClient bot, Telegram.Bot.Types.Update update)
        {
            var userId = update.Message.Chat.Id;
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            Log.Information("Получение данных для пользователя с ChatId: {ChatId}", userId);

            try
            {
                await bot.SendMessage(userId, "Данные обрабатываются...");

                var optionsBuilder = new DbContextOptionsBuilder<SupportJiraContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using (var context = new SupportJiraContext(optionsBuilder.Options))
                {
                    var codedUserId = EncodingToSha256.ComputeSha256Hash(Convert.ToString(userId));
                    var user = await context.AuthorizationJiras.FirstOrDefaultAsync(u => u.ChatId == codedUserId);

                    if (user == null)
                    {
                        Log.Warning("Пользователь с ChatId {ChatId} не найден.", codedUserId);

                        await bot.SendMessage(userId, "У вас нет доступа для работы с ботом! \n" +
                                                      "Для получения доступа введите token*<ваш токен jira>");

                        return "";
                    }

                    Log.Information("Пользователь найден: FirstName={FirstName}, LastName={LastName}", user.FirstName, user.LastName);

                    Console.WriteLine($"Пользователь: {user.LastName} {user.FirstName}");
                    return user.JiraToken;
                }
            }
            catch (DbUpdateException dbEx)
            {
                Log.Error(dbEx, "Ошибка базы данных при чтении записи для пользователя с ChatId: {ChatId}", userId);

                await bot.SendMessage(userId, "Произошла ошибка при работе с базой данных.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Непредвиденная ошибка при чтении записи для пользователя с ChatId: {ChatId}", userId);

                await bot.SendMessage(userId, "Произошла непредвиденная ошибка.");
            }

            return "";
        }

        public static async Task CreateUserInDB(ITelegramBotClient bot, Telegram.Bot.Types.Update update, string jiraToken, string fullname)
        {
            var userId = update.Message.Chat.Id;
            var firstName = fullname.Split(' ')[0];
            var lastName = fullname.Split(' ')[1];
            var codedUserId = EncodingToSha256.ComputeSha256Hash(Convert.ToString(userId));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            Log.Information("Начало создания записи для пользователя с ChatId: {ChatId}", userId);

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<SupportJiraContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using (var context = new SupportJiraContext(optionsBuilder.Options))
                {
                    var authorizationJira = new AuthorizationJira
                    {
                        FirstName = firstName,
                        UserName = $"@{update.Message.Chat.Username}",
                        LastName = lastName,
                        ChatId = codedUserId,
                        JiraToken = jiraToken,
                    };

                    await context.AuthorizationJiras.AddAsync(authorizationJira);
                    await context.SaveChangesAsync();
                    
                    Log.Information("Запись успешно добавлена для пользователя с ChatId: {ChatId}", userId);

                    await bot.SendMessage(userId, "Запись успешно добавлена!");
                }
            }
            catch (DbUpdateException dbEx)
            {
                Log.Error(dbEx, "Ошибка базы данных при создании записи для пользователя с ChatId: {ChatId}", userId);

                await bot.SendMessage(userId, "Не удалось создать запись! Ошибка базы данных.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Непредвиденная ошибка при создании записи для пользователя с ChatId: {ChatId}", userId);

                await bot.SendMessage(userId, "Не удалось создать запись! Непредвиденная ошибка.");
            }
        }
    }
}
