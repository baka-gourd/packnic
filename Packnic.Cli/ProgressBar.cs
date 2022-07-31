using System.Collections.Concurrent;

namespace Packnic.Cli;

public class ProgressBar
{
    public static void ResloveDependencies(string name, ref bool processing)
    {
        int gap = 0;
        while (processing)
        {
            gap++;
            var str = gap switch
            {
                <= 33 => "/",
                > 33 and <= 66 => "-",
                > 66 => "\\"
            };
            if (gap is 33 or 66 or 99)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\r{str}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" Resloving dependencies for ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(name);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("...");
            }

            if (gap is 99)
            {
                gap = 0;
            }

            Thread.Sleep(5);
        }
    }

    public static void DownloadOne(string name, ref bool processing)
    {
        int gap = 0;
        while (processing)
        {
            gap++;
            var str = gap switch
            {
                <= 33 => "/",
                > 33 and <= 66 => "-",
                > 66 => "\\"
            };
            if (gap is 33 or 66 or 99)
            {
                Console.Write("\r" + new string(' ', Console.BufferWidth));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\r{str}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" Downloading ");
                Console.ForegroundColor = ConsoleColor.Green;
                var width = Console.WindowWidth - 18;
                if (name.Length < width)
                {
                    Console.Write(name);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("...");
            }

            if (gap is 99)
            {
                gap = 0;
            }

            Thread.Sleep(5);
        }
    }

    public static void DownloadMany(IEnumerable<string> names, ref ConcurrentDictionary<string, bool> processing)
    {
        int gap = 0;
        while (processing.Values.Contains(true))
        {
            gap++;
            var str = gap switch
            {
                <= 33 => "/",
                > 33 and <= 66 => "-",
                > 66 => "\\"
            };
            if (gap is 33 or 66 or 99)
            {
                Console.Write("\r" + new string(' ', Console.BufferWidth));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\r{str}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" Downloading ");
                Console.ForegroundColor = ConsoleColor.Green;
                List<string> list = new();
                var width = Console.WindowWidth - 18;

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var s in names)
                {
                    if (processing[s] && width > 0)
                    {
                        var length = s.Length;
                        if (length >= width)
                        {
                            break;
                        }
                        list.Add(s);
                        width -= length + 2;
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Console.Write(list[i]);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (i != list.Count - 1)
                    {
                        Console.Write(", ");
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("...");
            }

            if (gap is 99)
            {
                gap = 0;
            }

            Thread.Sleep(5);
        }
    }
}