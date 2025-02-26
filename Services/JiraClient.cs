using its_bot.Models.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace its_bot.Services
{
    public class JiraClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly string _jiraBaseUrl;

        public JiraClient(IConfiguration configuration, string token)
        {
            _jiraBaseUrl = configuration["Jira:BaseUrl"];
            _token = token;

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
                        Console.WriteLine(contentDeserialize);
                        Console.WriteLine(JsonConvert.SerializeObject(contentDeserialize, Formatting.Indented));
                        return contentDeserialize;
                    }
                }
                else
                {

                    await bot.SendMessage(chatId, $"Ошибка - {response.StatusCode}");
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
                    Console.WriteLine("jsonIssue " + jsonIssue);
                    var content = new StringContent(jsonIssue, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url + "/rest/api/2/issue", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseIssue = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseIssue);
                    }
                    else
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(response));
                        Console.WriteLine($"Ошибка1: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка2: {ex.Message}");
                }
            }
        }

        public static async Task<CurrentUser?> getCurrentUser(string token, string url)
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
                        Console.WriteLine(content);
                        var contentDeserialize = JsonConvert.DeserializeObject<CurrentUser>(content);
                        Console.WriteLine(contentDeserialize);
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
    }

    public class Issue
    {
        public string Name { get; set; }
    }
}
