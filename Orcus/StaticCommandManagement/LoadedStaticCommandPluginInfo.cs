namespace Orcus.StaticCommandManagement
{
    public class LoadedStaticCommandPluginInfo
    {
        public LoadedStaticCommandPluginInfo(string filename,byte[] hash)
        {
            Filename = filename;
            Hash = hash;
        }

        public byte[] Hash { get;  }
        public string Filename { get; }
    }
}