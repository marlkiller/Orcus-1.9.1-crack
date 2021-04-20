namespace Orcus.Server.Core.Plugins
{
    public interface IUpdatePlugin
    {
        string Name { get; }
        string Host { get; }

        void ServerStarted();
        void Stop();
        bool SetupConsole();
        string SaveSettings();
        void LoadSettings(string settings);
    }
}