using System.ComponentModel.DataAnnotations.Schema;

namespace its_bot.Models
{
    [Table("AuthorizationJira")]
    public class AuthorizationJira
    {
        public int Id { get; set; }
        public string ChatId { get; set; }
        public string JiraToken { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
    }
}
