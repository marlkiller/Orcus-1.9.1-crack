namespace Orcus.Native
{
    internal static class ALG
    {
        public enum ALG_ID
        {
            CALG_MD5 = 0x00008003,
            CALG_SHA1 = ALG_CLASS_HASH | ALG_SID_SHA1
        }

        private const int ALG_CLASS_HASH = 4 << 13;
        private const int ALG_SID_SHA1 = 4;
    }
}