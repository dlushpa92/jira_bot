using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
namespace its_bot
{
    public class UserManager
    {
        public static async Task<string> GetUserFromDB(ITelegramBotClient bot, Telegram.Bot.Types.Update update, string jiraToken)
        {
            var userId = update.Message.Chat.Id;
            try
            {
                await bot.SendMessage(userId, "Данные обрабатываются...");
                using (var context = new SupportJiraContext())
                {
                    var context1 = context;
                    var authorizations = context.AuthorizationJiras.ToList(); // Получение всех записей
                    var codedUserId = EncodingToSha256.ComputeSha256Hash(Convert.ToString(userId));
                    var isExists = authorizations.Exists(user => user.ChatId == codedUserId);
                    Console.WriteLine("isExists   " + isExists);
                    //Console.WriteLine("hello   "+authorizations);
                    if (isExists == false)
                    {
                        await bot.SendMessage(userId, "У вас нет доступа для работы с ботом! \n" + "Для получения доступа введите token*<ваш токен jira>");
                    }
                    else
                    {
                        var user = authorizations[0];
                        ;
                        //var chatId = user.ChatId;
                        //jiraToken = user.JiraToken;
                        Console.WriteLine(userId);
                        ;
                        return user.JiraToken;
                        //await bot.SendMessage(userId, $"Добро пожаловать, {user.FirstName}!");
                        ;
                    }
                    return "";
                    //foreach (var authorization in authorizations)
                    //{
                    //    Console.WriteLine(1);
                    //    Console.WriteLine($"Id: {authorization.ChatId}, Username: {authorization.UserName}, JiraToken: {authorization.JiraToken}");
                    //}
                }
            }
            catch (Exception ex)
            {
                await bot.SendMessage(userId, "Не удалось получить данные!");
                Console.WriteLine("Ошибка  " + ex.Message);
                Console.WriteLine(ex.InnerException);
                return "";
            }
        }

        public static async Task CreateUserInDB(ITelegramBotClient bot, Telegram.Bot.Types.Update update, string jiraToken, string fullname)
        {
            var userId = update.Message.Chat.Id;
            var firstName = fullname.Split(' ')[0];
            var lastName = fullname.Split(' ')[1];
            var codedUserId = EncodingToSha256.ComputeSha256Hash(Convert.ToString(userId));
            ;
            try
            {
                using (var context = new SupportJiraContext())
                {
                    var authorizationJira = new AuthorizationJira
                    {
                        FirstName = firstName,
                        UserName = $"@{update.Message.Chat.Username}",
                        LastName = lastName,
                        ChatId = codedUserId,
                        JiraToken = jiraToken,
                        //Id = 1
                    };

                    context.AuthorizationJiras.Add(authorizationJira); // Добавление новой записи
                    context.SaveChanges(); // Сохранение изменений в базе данных
                    Console.WriteLine("Запись успешно добавлена.");
                }
            }
            catch (Exception ex)
            {
                await bot.SendMessage(userId, "Не удалось создать запись!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
               
            }
            finally
            {
                await bot.SendMessage(userId, "Запись успешно добавлена!");
            }
        }
    }
}
