using Orcus.Administration.Plugins.Administration;

namespace Orcus.Administration.Core.Plugins
{
    public class AdministrationControl : IAdministrationControl
    {
        public AdministrationControl(IAdministrationConnectionManager administrationConnectionManager)
        {
            AdministrationConnectionManager = administrationConnectionManager;
        }

        public IAdministrationConnectionManager AdministrationConnectionManager { get; }
        public IStaticCommander StaticCommander { get; }
    }
}