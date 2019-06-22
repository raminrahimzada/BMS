using System;

namespace BMS
{
    public static class Logger
    {
        public static void Debug(string line)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(line);
            Console.ForegroundColor = temp;
        }

        public static void Error(string line)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(line);
            Console.ForegroundColor = temp;
        }
    }
}