using System;

namespace Orcus.Server.CommandLine
{
    internal static class ConsoleHelper
    {
        public static bool GetYesNo(bool defaultValue, string text)
        {
            while (true)
            {
                Console.Write(text);
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return defaultValue;

                if (string.Equals(line, "y", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(line, "yes", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (string.Equals(line, "n", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(line, "no", StringComparison.OrdinalIgnoreCase))
                    return false;

                Console.WriteLine("Invalid input");
            }
        }

        public static string ReadNotNullString(string text)
        {
            while (true)
            {
                Console.Write(text);
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                    return line;
            }
        }

        public static string ReadNotNullString()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                    return line;
            }
        }

        public static int ReadInteger()
        {
            while (true)
            {
                var line = Console.ReadLine();
                int value;
                if (int.TryParse(line, out value))
                    return value;
                Console.WriteLine("Invalid input");
            }
        }

        public static int ReadInteger(string text)
        {
            while (true)
            {
                Console.Write(text);
                var line = Console.ReadLine();
                int value;
                if (int.TryParse(line, out value))
                    return value;
                Console.WriteLine("Invalid input");
            }
        }
    }
}