namespace Orcus.Administration.Core.Plugins
{
    public interface IPayload : IPlugin
    {
        long Size { get; }
        byte[] GetPayload();
    }
}