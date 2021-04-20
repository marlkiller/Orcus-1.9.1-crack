using System;
using System.Windows.Controls;
using Orcus.Administration.Plugins.Administration;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    /// You can use this class instead of <see cref="ICommandAndViewPlugin"/> for better type safety
    /// </summary>
    /// <typeparam name="TCommand">The command</typeparam>
    /// <typeparam name="TCommandView">The command view</typeparam>
    /// <typeparam name="TView">The view</typeparam>
    public class CommandAndViewPlugin<TCommand, TCommandView, TView> : ICommandAndViewPlugin where TCommand : Command
        where TCommandView : ICommandView
        where TView : UserControl
    {
        public Type Command { get; } = typeof (TCommand);
        public Type CommandView { get; } = typeof (TCommandView);
        public Type View { get; } = typeof (TView);

        public virtual void Initialize(IUiModifier uiModifier)
        {
        }
    }
}