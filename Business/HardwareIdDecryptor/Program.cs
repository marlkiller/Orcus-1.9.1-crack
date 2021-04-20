using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HardwareIdDecryptor
{
    class Program
    {
        // TOP SECRET
        public const string PrivateKey =
            "<RSAKeyValue><Modulus>0KP1FXkZN1mfNPh2+rOUUh+4GdH5Z0HEE99acDdwkjW0twzNUOJelpKZCDlDgPpbtfsTNzeaSe1gpSH+etfQMenfvNJRIYiM0llWinGCArGF3PlfmcCIxnQp40iBKrxB4vJJlI0bCmw4zXr0ofNB2Yx9qNDpVII+NUkQ+MAqOh8=</Modulus><Exponent>AQAB</Exponent><P>1nUu6zjdZlzw65mvu2e2Vf0ik/Y2+0BzBuxKG9lHxqOrFASYcAeKNus7LM8Tt+/PAmA2AgYeQOG614TuVDMGTQ==</P><Q>+Q5McctA0EM+DrAPwp7evh8syJDcGfG1pIPSzb/kFd2LrjANWC6aovlGsmtguDnjIDN8jp+cYxPzi5aCeVLQGw==</Q><DP>qMAC0GKpjG+bv7fqGMwOJPGP8N+v+dqH6K2rzugVW4/UAWdwT88PChHlOWgkevr/aD4uoT8RNTqqHAZkxupzjQ==</DP><DQ>MZbYP7whnpYU8CW8LZsmbKKWbyTwvjWzXhtlSNRRXM0s97TXo1w8LAEIQ5Q/wM6923aE8Xz0TYH2dy+zsOZ9Sw==</DQ><InverseQ>p6iMLbZ+wm0Cgh2RLsIZdQKiTe4H40ZavZiecnsJ8SCiJ2sw0bVQu4WjWBxXFDsi+eegTrjkrXBt3Asn5nzq6Q==</InverseQ><D>H/JrDa2sSchkU2UUOtESjfyjYPjA2qm8T1qs62/hwdGNaVYBfjgbSa0K17KE/pPXRSc7ywGtk1JQsRhicgbnruXqaG+Aqlh6Bo4AVHcx2Bf5WKDCmuRTb/eTUqenmGREiVkXa3QkpG5ij+05dcyT/zz9gSJX95CMxHMjxHxH5ak=</D></RSAKeyValue>";

        //TOP SECRET

        static void Main()
        {
            Console.WriteLine(
                "To decrypt a hardware ID, please type it here. If you don't know what to do, type \"help\"");
            var line = Console.ReadLine();
            if (string.Equals(line, "help", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine(
                    "To get the hardware id, follow this steps:\r\n1. Create a file in your temp (%temp%) directory called \"e3c6cefd462d48f0b30a5ebcd238b5b1\"\r\n2. Restart your PC\r\n3. Open the file with notepad and input the string here");
            else if (line != null)
            {
                using (var rsa = new RSACryptoServiceProvider(1024))
                {
                    rsa.FromXmlString(PrivateKey);
                    string result;

                    try
                    {
                        result =
                            new string(
                                Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(line), true))
                                    .Reverse()
                                    .ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        return;
                    }

                    Console.WriteLine("===============================================================================");
                    Console.WriteLine(result);
                    Console.WriteLine("===============================================================================");
                }
            }
        }
    }
}