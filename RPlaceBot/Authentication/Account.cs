using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RPlaceBot.Authentication
{
    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public CookieContainer CookieSet { get; set; }
        public JObject LoginResponseData { get;set; }
        public string ModHash { get; set; }
    }
}
