﻿using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Atlassian.Jira;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using its_bot.Models.Json;
using its_bot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;


namespace its_bot
{
    public class JiraClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly string _jiraBaseUrl;

        public JiraClient(string token)
        {
            var path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "config.json");
            var configJson = System.IO.File.ReadAllText(path);
            var appSettings = JsonConvert.DeserializeObject<AppSettings>(configJson);

            _token = token;
            _jiraBaseUrl = appSettings.BaseUrl;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        }

        public async Task<GetIssue> GetIssueAsync(string issueKey, ITelegramBotClient bot, Update update)
        {
            var chatId = update.Message.Chat.Id;
            try
            {
                var response = await _httpClient.GetAsync($"{_jiraBaseUrl}/rest/api/2/issue/{issueKey}");
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Console.WriteLine("Response content is empty or null.");
                        return null;
                    }
                    else
                    {
                        var contentDeserialize = JsonConvert.DeserializeObject<GetIssue>(content);
                        return contentDeserialize;
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await bot.SendMessage(chatId, "У вас нет прав");
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        await bot.SendMessage(chatId, "Задача не существует");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, $"Ошибка - {response.StatusCode}");
                    }

                    Console.WriteLine($"Error fetching issue: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"{ex.Message}");
                Console.WriteLine($"Exception while fetching issue: {ex.Message}");
            }
            return null;
        }

        public static async Task GetProject(string token, string url, string id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url + $"/rest/api/2/project/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);
                        var contentDeserialize = JsonConvert.DeserializeObject<Issue>(content);
                        Console.WriteLine("задача  -" + contentDeserialize);
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        public static async Task CreateIssue(string token, string url, string title, string description)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                try
                {
                    var issue = new
                    {
                        fields = new
                        {
                            project = new
                            {
                                id = 10149
                            },
                            summary = title,
                            issuetype = new
                            {
                                id = 10101
                            },
                            description = description
                        }
                    };
                    var jsonIssue = JsonConvert.SerializeObject(issue);
                    var content = new StringContent(jsonIssue, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url + "/rest/api/2/issue", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseIssue = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка1: {response.StatusCode}");
                        ;
                        var errorText = "Ничего не найдено";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка2: {ex.Message}");
                }
            }
        }

        public static async Task<CurrentUser> getCurrentUser(string token, string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url + $"/rest/api/2/myself");

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        var contentDeserialize = JsonConvert.DeserializeObject<CurrentUser>(content);
                        return contentDeserialize;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    return null;
                }
            }
        }

        public class Issue
        {
            public string Name { get; set; }
        }
    }
}

