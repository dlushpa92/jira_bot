using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace its_bot.Models.Json
{
    public class GetIssue
    {
        public string key { get; set; }
        public string self {  get; set; }
        public Fields fields {get; set; }
    }
    public class Fields
    {
        public string description { get; set; }
        public Assignee assignee { get; set; }

    }
    public class Assignee 
    {
        public string displayName {  get; set; } 
    }
}   
