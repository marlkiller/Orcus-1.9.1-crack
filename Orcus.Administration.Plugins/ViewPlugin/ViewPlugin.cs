using System;
using System.Windows.Controls;
using Orcus.Administration.Plugins.Administration;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace Orcus.Administration.Plugins.ViewPlugin
{
    /// <summary>
    ///     You can use this class instead of <see cref="IViewPlugin" /> for better type safety
    /// </summary>
    /// <typeparam name="TCommandView">The command view</typeparam>
    /// <typeparam name="TView">The view</typeparam>
    public class ViewPlugin<TCommandView, TView> : IViewPlugin where TCommandView : ICommandView
        where TView : UserControl
    {
        public Type CommandView { get; } = typeof (TCommandView);
        public Type View { get; } = typeof (TView);

        public virtual void Initialize(IUiModifier uiModifier)
        {
        }
    }
}