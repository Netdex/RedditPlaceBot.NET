using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BasebandProbe.Utility;
using Emgu.CV;
using Emgu.CV.Structure;
using RPlaceBot.Authentication;
using RPlaceBot.Graphical;
using RPlaceBot.Utility;
using SERAPH.Utility;

namespace RPlaceBot
{
    class Program
    {
        private static Account[] Accounts { get; set; }
        private static int[,] CurrentPreview { get; set; }
        private static Account DefaultAccount { get; set; }

        private static bool Running = true;

        private const bool DoLogin = true;
        private static double Cooldown = 5;

        static void Main(string[] args)
        {
            C.WriteLine("`i Importing accounts...");
            Accounts = AccountConfiguration.ReadXMLDocument();
            if (DoLogin)
            {
                C.WriteLine("`i Logging into accounts...");
                LoginManager.SetCookieData(Accounts);
                C.WriteLine("`i Logged into following accounts:");
                foreach (var account in Accounts)
                {
                    C.WriteLine($":: USERNAME \t{account.Username}\n" +
                                $"   ERR_STATUS \t{ObjectDumper.Dump(account.LoginResponseData["json"]["errors"])}");
                }
                DefaultAccount = Accounts[0];
            }
            //Console.WriteLine(PlaceRequest.Draw(Accounts[0], 0, 0, 0));

            Image<Bgra, byte> image;

            C.WriteLine("Define image path:");
            string imgPath = Console.ReadLine();
            image = new Image<Bgra, byte>(imgPath);


            C.WriteLine("`i The image you provided will look something like this:");
            var quant = ImageConvert.QuantifyImage(image);
            ImageConvert.QuantifyPrint(quant);

            C.WriteLine("Define coordinates [x y]:");
            string udd = Console.ReadLine();

            int imgX, imgY;
            UpdatePreview(DefaultAccount);
            if (udd == "optimize")
            {
                C.WriteLine("`i Optmizing...");
                for (int y = 0; y < 1000 - quant.GetLength(1); y++)
                {
                    for (int x = 0; x < 1000 - quant.GetLength(0); x++)
                    {
                        double time = TimeRequired(quant, x, y);
                        if (time == 0)
                            for (int dx = 0; dx < quant.GetLength(0); dx++)
                                for (int dy = 0; dy < quant.GetLength(1); dy++)
                                    CurrentPreview[x + dx, y + dy] = -1;
                    }
                }
                int mx = 0, my = 0;
                double mt = double.MaxValue;
                for (int y = 0; y < 1000 - quant.GetLength(1); y++)
                {
                    for (int x = 0; x < 1000 - quant.GetLength(0); x++)
                    {
                        double time = TimeRequired(quant, x, y);
                        if (time < mt && time != 0)
                        {
                            mx = x;
                            my = y;
                            mt = time;
                        }
                    }
                }
                imgX = mx;
                imgY = my;
                C.WriteLine($"`i Optmized coordinates at ({mx},{my}), would take {mt} min(s)");
            }
            else
            {
                string[] u_ccd = udd.Split(' ');
                imgX = int.Parse(u_ccd[0]);
                imgY = int.Parse(u_ccd[1]);
            }
            C.WriteLine("`i The area you are replacing looks like this:");
            ImageConvert.QuantifyPrint(CurrentPreview, imgX, imgY, quant.GetLength(0), quant.GetLength(1));
            C.WriteLine("`i Press any key to continue...");
            Console.ReadKey();

            Thread[] Executors = new Thread[Accounts.Length];
            for (int i = 0; i < Accounts.Length; i++)
            {
                var index = i;
                Executors[i] = new Thread(() =>
                {
                    var account = Accounts[index];
                    C.WriteLine($"`i Executor {index} is running on account {account.Username}");
                    while (Running)
                    {
                        CurrentPreview = PlaceRequest.GetBoardState(account);

                        List<Tuple<int, int>> incorrect = new List<Tuple<int, int>>();
                        for (int y = 0; y < quant.GetLength(1); y++)
                            for (int x = 0; x < quant.GetLength(0); x++)
                                if (quant[x, y] != -1 && quant[x, y] != CurrentPreview[imgX + x, imgY + y])
                                    incorrect.Add(new Tuple<int, int>(x, y));
                        int minX = -1, minY = -1;
                        int centerX = quant.GetLength(0) / 2, centerY = quant.GetLength(1) / 2;
                        int manhattan = int.MaxValue;
                        foreach (var tup in incorrect)
                        {
                            int m = Math.Abs(centerX - tup.Item1) + Math.Abs(centerY - tup.Item2);
                            if (m < manhattan)
                            {
                                manhattan = m;
                                minX = tup.Item1;
                                minY = tup.Item2;
                            }
                        }
                        int wait = 45;
                        if (minX != -1 && minY != -1)
                        {
                            wait = PlaceRequest.Draw(account, imgX + minX, imgY + minY, quant[minX, minY]);
                            if (wait < 0)
                            {
                                wait *= -1;
                                C.WriteLine($"`i {index}: Could not draw pixel at ({imgX + minX},{imgY + minY})!");
                            }
                            else
                            {
                                C.WriteLine($"`i {index}: Drew pixel at ({imgX + minX},{imgY + minY})!");
                                Cooldown = wait / 60.0;
                            }
                        }
                        C.WriteLine($"`i {index}: Waiting {wait} seconds");
                        Thread.Sleep(wait * 1000 + 1000);
                    }
                });
                Executors[i].Start();
                Thread.Sleep(10000);
            }
            var watch = new Thread(() =>
            {
                while (Running)
                {
                    double time = TimeRequired(quant, imgX, imgY);
                    C.WriteLine($"`i Estimated time required: {time} min(s)");
                    Thread.Sleep((int)(Cooldown * 60 * 1000));
                    C.WriteLine("`i STATE_WATCH: Current state:");
                    ImageConvert.QuantifyPrint(CurrentPreview, imgX, imgY, quant.GetLength(0), quant.GetLength(1));
                }
            });
            watch.Start();

            C.WriteLine("`i Press any key to stop!");
            Console.ReadKey();

            Running = false;
            for (int i = 0; i < Executors.Length; i++)
                Executors[i].Abort();
            watch.Abort();
        }

        public static double TimeRequired(int[,] quant, int ax, int ay)
        {
            double time = 0;
            for (int y = 0; y < quant.GetLength(1); y++)
                for (int x = 0; x < quant.GetLength(0); x++)
                    if (quant[x, y] != -1 && quant[x, y] != CurrentPreview[ax + x, ay + y])
                        time += Cooldown;
            time /= Accounts.Length;
            return time;
        }
        public static void UpdatePreview(Account account)
        {
            CurrentPreview = PlaceRequest.GetBoardState(account);
        }
    }
}
