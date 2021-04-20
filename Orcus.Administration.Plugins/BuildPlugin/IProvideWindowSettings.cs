using System.Windows;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;

namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Provide settings using a custom window. Warning: These settings won't be saved in a build configuration. Please
    ///     consider using <see cref="IProvideBuilderSettings" /> or <see cref="IProvideEditableProperties" />
    /// </summary>
    public interface IProvideWindowSettings
    {
        /// <summary>
        ///     Open the settings
        /// </summary>
        /// <param name="ownerWindow">The owner window. Set this as the <see cref="Window.Owner" /> property of the custom window</param>
        void ShowSettingsWindow(IWindowService ownerWindow);
    }
}