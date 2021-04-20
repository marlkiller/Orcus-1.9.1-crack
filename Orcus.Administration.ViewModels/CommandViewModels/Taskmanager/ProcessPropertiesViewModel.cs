using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.TaskManager;

namespace Orcus.Administration.ViewModels.CommandViewModels.Taskmanager
{
    public class ProcessPropertiesViewModel
    {
        public ProcessPropertiesViewModel(AdvancedProcessInfo processInfo, AdvancedProcessInfo parentProcessInfo)
        {
            if (parentProcessInfo == null && processInfo.Id == 0)
            {
                ParentProcess = (string) Application.Current.Resources["None"];
            }
            else if (parentProcessInfo == null)
            {
                ParentProcess = $"<{Application.Current.Resources["NonExistentProcess"]}> ({processInfo.ParentProcess})";
            }
            else
            {
                ParentProcess = $"{parentProcessInfo.Name} ({parentProcessInfo.Id})";
            }
            Icon = processInfo.Icon ??
                   new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/VisualStudio/Property.ico",
                       UriKind.Absolute));
            ProcessInfo = processInfo;
        }

        public AdvancedProcessInfo ProcessInfo { get; }
        public ImageSource Icon { get; }
        public string ParentProcess { get; }
    }
}