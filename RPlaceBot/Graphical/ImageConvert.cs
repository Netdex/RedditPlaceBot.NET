using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace RPlaceBot.Graphical
{
    class ImageConvert
    {
        public static Bgra[] Colors = new Bgra[]
        {
            new Bgra(255,255,255,255),
            new Bgra(228,228,228,255),
            new Bgra(136,136,136,255),
            new Bgra(34,34,34,255),
            new Bgra(209,167,255,255),
            new Bgra(0,0,229,255),
            new Bgra(0,149,229,255),
            new Bgra(66,106,160,255),
            new Bgra(0,217,229,255),
            new Bgra(68,224,148,255),
            new Bgra(1,190,2,255),
            new Bgra(221,211,0,255),
            new Bgra(199,131,0,255),
            new Bgra(234,0,0,255),
            new Bgra(228,110,207,255),
            new Bgra(128,0,130,255),
        };

        public static ConsoleColor[] RespectiveColor = new ConsoleColor[]
        {
            ConsoleColor.White,
            ConsoleColor.Gray,
            ConsoleColor.DarkGray,
            ConsoleColor.Black,
            ConsoleColor.Magenta,
            ConsoleColor.DarkRed,
            ConsoleColor.Red,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkYellow,
            ConsoleColor.Yellow,
            ConsoleColor.Green,
            ConsoleColor.DarkGreen,
            ConsoleColor.Cyan,
            ConsoleColor.DarkCyan,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.DarkMagenta
        };

        public static void QuantifyPrint(int[,] u, int x, int y, int width, int height)
        {
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    if (u[x + dx, y + dy] == -1)
                        Console.BackgroundColor = ConsoleColor.Black;
                    else
                        Console.BackgroundColor = RespectiveColor[u[x + dx, y + dy]];
                    Console.Write("  ");
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        public static void QuantifyPrint(int[,] u)
        {
            QuantifyPrint(u, 0, 0, u.GetLength(0), u.GetLength(1));
        }

        public static int[,] QuantifyImage(Image<Bgra, byte> u)
        {
            return QuantifyImage(u, 0, 0, u.Width, u.Height);
        }

        public static int[,] QuantifyImage(Image<Bgra, byte> u, int x, int y, int width, int height)
        {
            int[,] q = new int[width, height];
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    if (u[y + dy, x + dx].Alpha == 0)
                        q[dx, dy] = -1;
                    else
                        q[dx, dy] = FindClosestColor(u[y + dy, x + dx]);

                }
            }
            return q;
        }

        public static Image<Bgra, byte> QuantifyToImage(int[,] u)
        {
            return QuantifyToImage(u, 0, 0, u.GetLength(0), u.GetLength(1));
        }

        public static Image<Bgra, byte> QuantifyToImage(int[,] u, int x, int y, int width, int height)
        {
            Image<Bgra, byte> q = new Image<Bgra, byte>(new Size(width, height));
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    if (u[dx + x, dy + y] == -1)
                        q[dy, dx] = new Bgra(0, 0, 0, 0);
                    else
                        q[dy, dx] = Colors[u[dx + x, dy + y]];
                }
            }
            return q;
        }

        public static int FindClosestColor(Bgra a)
        {
            int minIdx = 0;
            double minDist = double.MaxValue;
            for (int i = 0; i < Colors.Length; i++)
            {
                double dist = ColorDistanceSq(a, Colors[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = i;
                }
            }
            return minIdx;
        }

        public static double ColorDistanceSq(Bgra a, Bgra b)
        {
            return (a.Red - b.Red) * (a.Red - b.Red) + (a.Blue - b.Blue) * (a.Blue - b.Blue) +
                   (a.Green - b.Green) * (a.Green - b.Green);
        }
    }
}
