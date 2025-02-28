using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace its_bot.Services
{
    public class MessageRemove
    {
        private static long _chatId { get; set; }
        private static int _messageId { get; set; }
        public static Dictionary<string, int> _state { get; set; }

        public MessageRemove()
        {
            //_chatId = chatId;
            //_messageId = messageId;
            _state = new Dictionary<string, int>();
        }

        public  static void Add(string nameOfMessage, int messageId)
        {
            _state.Add(nameOfMessage, messageId);
        }

        public static int GetMessageId(string nameOfMessage)
        {
            int messageId = 0;
            _state.TryGetValue(nameOfMessage, out messageId);
            Console.WriteLine(messageId);
            return messageId;
        }
        public static void Remove(string nameOfMessage) 
        {
            if (_state.ContainsKey(nameOfMessage))
            {
                if (nameOfMessage == "*")
                {
                    _state.Clear();
                }
                else
                {

                    _state.Remove(nameOfMessage);

                }
            }
        }
    }
}
