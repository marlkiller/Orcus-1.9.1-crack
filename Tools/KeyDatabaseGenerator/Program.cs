using System;
using System.IO;
using System.Linq;
using System.Text;
using Orcus.Shared.Encryption;

namespace KeyDatabaseGenerator
{
    internal class Program
    {
        private const string Password =
            "@=<VY]BUQM{sp&hH%xbLJcUd/2sWgR+YA&-_Z>/$skSXZR!:(yZ5!>t>ZxaPTrS[Z/'R,ssg'.&4yZN?S)My+:QV2(c&x/TU]Yq2?g?*w7*r@pmh";

        private const int PasswordCount = 1000;
        private const int PasswordLength = 32;

        private static void Main()
        {
            var random = new Random();
            Console.WriteLine("Gernerating passwords...");
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < PasswordCount; i++)
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var result = new string(
                    Enumerable.Repeat(chars, PasswordLength)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());

                stringBuilder.AppendLine($"\"{AES.Encrypt(result, Password)}\",");
                Console.Write($"\r{i + 1} of {PasswordCount} generated");
            }
            Console.WriteLine();

            Console.WriteLine("Successfully generated");

            File.WriteAllText("KeyDatabase.cs",
                Properties.Resources.KeyDatabaseClass.Replace("$keys", "\r\n" + stringBuilder + "\r\n"));
            Console.WriteLine("Saved to \"KeyDatbase.cs\"");
            Console.ReadKey();
        }
    }
}