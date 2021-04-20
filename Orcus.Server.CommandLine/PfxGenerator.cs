using System;
using System.Collections;
using System.Security.Cryptography;
using Mono.Security.X509;

namespace Orcus.Server.CommandLine
{
    public class PfxGenerator
    {
        //adapted from https://github.com/mono/mono/blob/master/mcs/tools/security/makecert.cs
        public static byte[] GeneratePfx(string certificateName, string password)
        {
            byte[] sn = GenerateSerialNumber();
            string subject = $"CN={certificateName}";

            DateTime notBefore = DateTime.Now;
            DateTime notAfter = DateTime.Now.AddYears(20);

            RSA subjectKey = new RSACryptoServiceProvider(2048);


            string hashName = "SHA256";

            X509CertificateBuilder cb = new X509CertificateBuilder(3)
            {
                SerialNumber = sn,
                IssuerName = subject,
                NotBefore = notBefore,
                NotAfter = notAfter,
                SubjectName = subject,
                SubjectPublicKey = subjectKey,
                Hash = hashName
            };

            byte[] rawcert = cb.Sign(subjectKey);

            PKCS12 p12 = new PKCS12 {Password = password};

            Hashtable attributes = GetAttributes();

            p12.AddCertificate(new X509Certificate(rawcert), attributes);
            p12.AddPkcs8ShroudedKeyBag(subjectKey, attributes);

            return p12.GetBytes();
        }

        private static Hashtable GetAttributes()
        {
            ArrayList list = new ArrayList {new byte[] {1, 0, 0, 0}};
            // we use a fixed array to avoid endianess issues 
            // (in case some tools requires the ID to be 1).
            Hashtable attributes = new Hashtable(1) {{PKCS9.localKeyId, list}};
            return attributes;
        }

        private static byte[] GenerateSerialNumber()
        {
            byte[] sn = Guid.NewGuid().ToByteArray();

            //must be positive
            if ((sn[0] & 0x80) == 0x80)
                sn[0] -= 0x80;
            return sn;
        }

        public static byte[] GetCertificateForBytes(byte[] pfx, string password)
        {
            var pkcs = new PKCS12(pfx, password);
            var cert = pkcs.GetCertificate(GetAttributes());

            return cert.RawData;
        }
    }
}