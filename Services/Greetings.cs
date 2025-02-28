using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace its_bot.Services
{
    internal class Greetings
    {
        private static string[] _userName {  get; set; }
        public Greetings(string[] userName) 
        {
            _userName = userName;
        }

        public string greetingsUser()
        {
            string[] welcome = { "Доброго утра", "Доброго дня", "Доброго вечера" };
            var time = DateTime.Now.ToString("t");
            var hour = int.Parse(time.Split(":")[0]);
            var greetings = "";

            if (hour >= 4 && hour < 12)
            {
                return greetings = $"{welcome[0]} {_userName[0]}";
            }
            else if (hour >= 12 && hour < 18)
            {
                return greetings = $"{welcome[1]} {_userName[0]}";
            }
            else if (hour >= 18 && hour < 22)
            {
                return greetings = $"{welcome[2]} {_userName[0]}";
            }
            else
            {
                return greetings = $"Привет {_userName[0]}";
            }
        } 
    }
}