﻿using System.Net.Http.Json;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using its_bot.Models;
using its_bot;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace its_bot
{
    public class UpdateHandler : IUpdateHandler
    {
        private static string jiraToken = "";
        private static Dictionary<long, UserSession> userSessions = new Dictionary<long, UserSession>();
        public event Action<string> OnHandleUpdateStarted;
        public event Action<string> OnHandleUpdateCompleted;
        public event Action<string> OnHandleUpdateCallback;

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            string message = "";

            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                message = update.Message.Text;
                Console.WriteLine(message);
                OnHandleUpdateStarted?.Invoke(message);
            } 
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Message != null)
            {
                message = update.CallbackQuery.Message.Text;
                OnHandleUpdateCallback?.Invoke(message);
            } 
            else
            {
                return;
            }

            try
            {
                if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    var msg = update.CallbackQuery.Message;
                    if (msg != null)
                    {
                        await HandleCallbackQueryAsync(bot, update.CallbackQuery, update);
                    }
                }
                else if (update.Type == UpdateType.Message && update.Message?.Text?.Contains("token*") == true)
                {
                    var text = update.Message.Text.Split('*');
                    var userId = update.Message.Chat.Id;
                    var gottenJiraToken = "";

                    if (text.Length > 1)
                    {
                        gottenJiraToken = text[1];
                        var url = await GetUrl();
                        var curentUser = await its_bot.JiraClient.getCurrentUser(gottenJiraToken, url);

                        if (curentUser == null)
                        {
                            await bot.SendMessage(userId, "Введенный токен не валиден!");
                        }
                        else
                        {
                            await UserManager.CreateUserInDB(bot, update, gottenJiraToken, curentUser.displayName);
                        }
                    }
                    else
                    {
                        await bot.SendMessage(userId, "Введенный токен не валидный!");
                    }
                }

                else if (update.Type == UpdateType.Message && update.Message?.Text?.StartsWith("/") == true)
                {
                    await OnCommand(update.Message.Text, update, bot);
                }
                else if (update.Type == UpdateType.Message && update.Message?.Text?.StartsWith("*") == true)
                {
                    var chatId = update.Message.Chat.Id;
                    var userText = update.Message.Text;
                    var issueNumber = userText.Replace("*", "").ToUpper();
                    var jiraClient = new JiraClient(jiraToken);

                    try
                    {
                        var getIssue = await jiraClient.GetIssueAsync(issueNumber, bot, update);
                        if (getIssue != null)
                        {
                            var path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "config.json");
                            var configJson = System.IO.File.ReadAllText(path);
                            var appSettings = JsonConvert.DeserializeObject<AppSettings>(configJson);

                            var textToSend = $"задача: {getIssue.key}\n" +
                                $"описание: {(string.IsNullOrEmpty(getIssue.fields.description) ? "Нет описания" : getIssue.fields.description)}\n" +
                                $"ссылка: {appSettings.BaseUrl}/browse/{getIssue.key.ToUpper()}\n" +
                                $"ответственный: {(string.IsNullOrEmpty(getIssue.fields.assignee?.displayName) ? "нет ответственного" : getIssue.fields.assignee.displayName)}";
                            await bot.SendMessage(chatId, textToSend);
                        }
                    }
                    catch (Exception ex)
                    {
                        await bot.SendMessage(update.Message.Chat.Id, ex.Message);
                    }


                }
                else if (update.Type == UpdateType.Message && userSessions.ContainsKey(update.Message.Chat.Id))
                {
                    var userId = update.Message.Chat.Id;
                    var session = userSessions[userId];
                    var currentStep = session.Step;
                    var text = update.Message.Text;

                    if (currentStep == "begin")
                    {
                        session.TaskTitle = text;
                        session.Step = "has_title";
                        await bot.SendMessage(userId, "Введите описание задачи");
                    }
                    else if (currentStep == "has_title")
                    {
                        session.TaskDescription = text;
                        session.Step = "ready_to_create";

                        var inlineMarkup = new InlineKeyboardMarkup()
                            .AddNewRow()
                                .AddButton("Создать задачу", "createIssue")
                                .AddButton("Не создавать", "dontCreateIssue");
                        var createIssueText = "Создать задачу? \n " +
                            $"заголовок: {session.TaskTitle} \n" +
                            $"описание: {session.TaskDescription}";
                        await bot.SendMessage(userId, createIssueText, replyMarkup: inlineMarkup);
                    }

                    userSessions[userId] = session;
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки сообщения: {ex.Message}");
            }
            finally
            {
                OnHandleUpdateCompleted?.Invoke(message);
            }
        }

        async Task OnCommand(string command, Telegram.Bot.Types.Update update, ITelegramBotClient bot)
        {
            var msg = update.Message;
            var userId = update.Message.Chat.Id;

            switch (command)
            {
                case "/start":
                    await UserManager.GetUserFromDB(bot, update, jiraToken, "start");
                    break;

                case "/jira":
                    jiraToken = await UserManager.GetUserFromDB(bot, update, jiraToken, "jira");
                    
                    if (jiraToken != "")
                    {
                        var inlineMarkup = new InlineKeyboardMarkup()
                            .AddNewRow()
                                .AddButton("Создать задачу", "%createissue")
                                .AddButton("Найти задачу", "%getissue");
                        var message = await bot.SendMessage(userId, "Выберите действие:", replyMarkup: inlineMarkup);
                    }
                    break;

                case "/info":
                    await bot.SendMessage(msg.Chat.Id, "Чтобы начать напишите /start, \n чтобы вызвать меню наберите /jira");
                    break;
            }
        }

        async Task HandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, Telegram.Bot.Types.Update update)
        {
            var callbackMsg = update.CallbackQuery.Message;
            var msg = update.Message;
            if (callbackQuery.Data == "%getissue")
            {
                await bot.SendMessage(callbackMsg.Chat.Id, "Введите * + номер задачи без пробелов, \n" + "пример: *plb-1");
            }
            else if (callbackQuery.Data == "%createissue")
            {
                long userId = callbackMsg.Chat.Id;
                var session = new UserSession();
                session.Step = "begin";

                if (userSessions.ContainsKey(userId))
                {
                    userSessions[userId] = session;
                }
                else
                {
                    userSessions.Add(userId, session);
                }

                await bot.SendMessage(userId, "Введите заголовок задачи");

            }
            else if (callbackQuery.Data == "createIssue")
            {
                long userId = callbackMsg.Chat.Id;
                var session = userSessions[userId];
                try
                {
                    var path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "config.json");
                    var appSettingsJson = System.IO.File.ReadAllText(path);
                    var deserialize = JsonConvert.DeserializeObject<AppSettings>(appSettingsJson);
                    var token = jiraToken;
                    var jiraBaseUrl = deserialize.BaseUrl;
                    await JiraClient.CreateIssue(token, jiraBaseUrl, session.TaskTitle, session.TaskDescription);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                session.Step = "completed";
                await bot.SendMessage(userId, "Задача создана!");
            }
            else if (callbackQuery.Data == "dontCreateIssue")
            {
                long userId = callbackMsg.Chat.Id;
                await bot.SendMessage(userId, "Создание задачи отменено!");
            }

        }
        async Task<string> GetUrl()
        {
            var path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "config.json");
            var appSettingsJson = System.IO.File.ReadAllText(path);
            var deserialize = JsonConvert.DeserializeObject<AppSettings>(appSettingsJson);
            var jiraBaseUrl = deserialize.BaseUrl;
            return jiraBaseUrl;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Произошла ошибка: {exception.Message}");
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
