using Orcus.Administration.Plugins.Administration;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     A plugin which can make changes to the UI of the administration (e. g. add menu items)
    /// </summary>
    public interface IAdministrationPlugin
    {
        /// <summary>
        ///     Initialize the plugin
        /// </summary>
        /// <param name="uiModifier">The UI modifier allows changes to the user interface of the administration</param>
        /// <param name="administrationControl">Access to some features of the administration</param>
        void Initialize(IUiModifier uiModifier, IAdministrationControl administrationControl);
    }
}