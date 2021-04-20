using System;
using System.Security.Cryptography;

namespace Orcus.Commands.Passwords.Applications.Mozilla.Cryptography
{
    public class MozillaPBE
    {
        public MozillaPBE(byte[] globalSalt, byte[] masterPassword, byte[] entrySalt)
        {
            GlobalSalt = globalSalt;
            MasterPassword = masterPassword;
            EntrySalt = entrySalt;
        }

        private byte[] GlobalSalt { get; }
        private byte[] MasterPassword { get; }
        private byte[] EntrySalt { get; }
        public byte[] Key { get; private set; }
        public byte[] IV { get; private set; }

        public void Compute()
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] GLMP; // globalSalt + masterPassword
            byte[] HP; // SHA1(GLMP)
            byte[] HPES; // HP + entrySalt
            byte[] CHP; // SHA1(HPES)
            byte[] PES; // entrySalt completed to 20 bytes by zero
            byte[] PESES; // PES + entrySalt
            byte[] k1;
            byte[] tk;
            byte[] k2;
            byte[] k; // final value conytaining key and iv

            //GLMP
            GLMP = new byte[GlobalSalt.Length + MasterPassword.Length];
            Array.Copy(GlobalSalt, 0, GLMP, 0, GlobalSalt.Length);
            Array.Copy(MasterPassword, 0, GLMP, GlobalSalt.Length, MasterPassword.Length);

            //HP
            HP = sha.ComputeHash(GLMP);
            //HPES
            HPES = new byte[HP.Length + EntrySalt.Length];
            Array.Copy(HP, 0, HPES, 0, HP.Length);
            Array.Copy(EntrySalt, 0, HPES, HP.Length, EntrySalt.Length);
            //CHP
            CHP = sha.ComputeHash(HPES);
            //PES
            PES = new byte[20];
            Array.Copy(EntrySalt, 0, PES, 0, EntrySalt.Length);
            for (int i = EntrySalt.Length; i < 20; i++)
            {
                PES[i] = 0;
            }
            //PESES
            PESES = new byte[PES.Length + EntrySalt.Length];
            Array.Copy(PES, 0, PESES, 0, PES.Length);
            Array.Copy(EntrySalt, 0, PESES, PES.Length, EntrySalt.Length);

            using (HMACSHA1 hmac = new HMACSHA1(CHP))
            {
                //k1
                k1 = hmac.ComputeHash(PESES);
                //tk
                tk = hmac.ComputeHash(PES);
                //tkES
                byte[] tkEs = new byte[tk.Length + EntrySalt.Length];
                Array.Copy(tk, 0, tkEs, 0, tk.Length);
                Array.Copy(EntrySalt, 0, tkEs, tk.Length, EntrySalt.Length);
                //k2
                k2 = hmac.ComputeHash(tkEs);
            }

            //k
            k = new byte[k1.Length + k2.Length];
            Array.Copy(k1, 0, k, 0, k1.Length);
            Array.Copy(k2, 0, k, k1.Length, k2.Length);

            Key = new byte[24];

            for (int i = 0; i < Key.Length; i++)
            {
                Key[i] = k[i];
            }

            IV = new byte[8];
            int j = IV.Length - 1;

            for (int i = k.Length - 1; i >= k.Length - IV.Length; i--)
            {
                IV[j] = k[i];
                j--;
            }
        }
    }
}