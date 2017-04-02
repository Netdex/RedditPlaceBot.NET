using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TAAPI;

namespace RPlaceBot.Authentication
{
    class PlaceRequest
    {
        private static readonly Tuple<int,int> HardcodedSize = new Tuple<int, int>(1000, 1000);

        public static TWebClient Init()
        {
            TWebClient Client;
            Client = new TWebClient();
            Client.Headers["user-agent"] =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            return Client;
        }

        public static void TransferState(TWebClient Client, Account account)
        {
            Client.CookieContainer = account.CookieSet;
            Client.Headers["x-modhash"] = account.ModHash;
        }

        public static int Draw(Account account, int x, int y, int c)
        {
            var Client = Init();
            TransferState(Client, account);

            var data = new NameValueCollection();
            data["x"] = x + "";
            data["y"] = y + "";
            data["color"] = c + "";
            bool err = false;
            string response;
            try
            {
                response =
                    Encoding.ASCII.GetString(Client.UploadValues("https://www.reddit.com/api/place/draw.json", "POST",
                        data));
            }
            catch (WebException e)
            {
                response = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                err = true;
            }

            JObject json = JObject.Parse(response);
            int wait = (int)double.Parse((string) json["wait_seconds"]);
            if (err) return -wait;
            return wait;
        }

        public static int[,] GetBoardState(Account account)
        {
            var Client = Init();
            TransferState(Client, account);

            try
            {
                byte[] urawdata = Client.DownloadData("https://www.reddit.com/api/place/board-bitmap");
                byte[] rawdata = new byte[urawdata.Length - 4];
                Array.Copy(urawdata, 4, rawdata, 0, rawdata.Length);
                int[,] data = new int[HardcodedSize.Item1, HardcodedSize.Item2];
                for (int i = 0; i < rawdata.Length; i++)
                {
                    int j = 2 * i;
                    if (j >= HardcodedSize.Item1 * HardcodedSize.Item2)
                        break;
                    int kx = j % HardcodedSize.Item2, ky = j / HardcodedSize.Item1;
                    int v = rawdata[i] >> 4;
                    data[kx, ky] = v;
                    j = 2 * i + 1;
                    if (j >= HardcodedSize.Item1 * HardcodedSize.Item2)
                        break;
                    kx = j % HardcodedSize.Item2;
                    ky = j / HardcodedSize.Item1;
                    v = rawdata[i] & 15;
                    data[kx, ky] = v;
                }
                return data;
            }
            catch (WebException)
            {
                return new int[HardcodedSize.Item1, HardcodedSize.Item2];
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
