using System.Windows;
using System.Windows.Media;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace Orcus.Administration.ViewModels
{
    public class CommandViewModel
    {
        public CommandViewModel(FrameworkElement view, ICommandView commandView)
        {
            View = view;
            Icon = commandView.Icon;
        }

        public FrameworkElement View { get; }
        public ImageSource Icon { get;  }
    }
}