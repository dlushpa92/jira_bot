using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace its_bot
{
    [Table("AuthorizationJira")]
    public class AuthorizationJira
    {
        public int Id {  get; set; }
        public string ChatId {  get; set; }
        public string JiraToken { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
    }
}
