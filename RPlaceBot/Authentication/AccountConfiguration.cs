using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RPlaceBot.Authentication
{
    class AccountConfiguration
    {
        public static Account[] ReadXMLDocument()
        {
            try
            {
                var xmld = new XmlDocument();
                xmld.Load("RedditAccounts.xml");

                var accounts = new List<Account>();
                foreach (XmlNode account in xmld.DocumentElement.SelectNodes("./Account"))
                {
                    string username = account["Username"].InnerText;
                    string password = account["Password"].InnerText;
                    accounts.Add(new Account() { Username = username, Password = password });
                }
                return accounts.ToArray();
            }
            catch (Exception)
            {
                throw new XmlSyntaxException("Invalid XML account data");
            }
        }
    }
}
