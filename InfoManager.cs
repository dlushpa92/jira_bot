using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace its_bot
{
    public class InfoManager
    {
        public static string getToken()
        {
            var token = "";
            try
            {
                var path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "token.txt");
                token = File.ReadAllText(path, Encoding.Unicode);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
            return token;
        }

        public static string getServerInfo()
        {
            var serverSettings = "";
            try
            {
                var path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "serverSettings.txt");
                serverSettings = File.ReadAllText(path, Encoding.Unicode).Replace("\r", "").Replace("\n", "");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return serverSettings;
        }
    }
}
