using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace its_bot.Models
{
    enum QuestionStep
    {
        AskTaskTitle,
        AskTaskDescription,
        Done
    }

    class UserSession
    {
        public string Step {  get; set; }
        public string TaskTitle { get; set; }        // Название задачи
        public string TaskDescription { get; set; }  // Описание задачи
    }
}
