using System.Windows;

namespace Orcus.Administration.ViewModels.Controller
{
    public interface IViewManagerModelController
    {
        FrameworkElement GetView(object viewModel);
        FrameworkElement GetNewView(object viewModel);
    }
}