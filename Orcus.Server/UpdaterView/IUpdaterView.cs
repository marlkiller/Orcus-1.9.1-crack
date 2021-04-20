using Orcus.Server.Core.Plugins;

namespace Orcus.Server.UpdaterView
{
    public interface IUpdaterView
    {
        void Initizalize(IUpdatePlugin plugin);
        bool ValidateInputs();
    }
}