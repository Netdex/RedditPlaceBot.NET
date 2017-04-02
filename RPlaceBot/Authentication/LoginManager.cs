using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TAAPI;

namespace RPlaceBot.Authentication
{
    class LoginManager
    {
        public static void SetCookieData(Account[] accounts)
        {
            foreach (var account in accounts)
            {
                var client = new TWebClient();
                client.Headers["user-agent"] =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
                var loginData = new NameValueCollection();
                loginData["op"] = "login";
                loginData["user"] = account.Username;
                loginData["passwd"] = account.Password;
                loginData["api_type"] = "json";
                
                // loginData["rem"] = "on";
                string result = Encoding.ASCII.GetString(client.UploadValues($"https://www.reddit.com/api/login/{account.Username}", "POST", loginData));
                account.CookieSet = client.CookieContainer;
                account.LoginResponseData = JObject.Parse(result);
                account.ModHash = (string) account.LoginResponseData["json"]["data"]["modhash"];
            }
        }
    }
}
