using System.Windows;
using System.Windows.Controls;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Extensions
{
    public class ClientInformationDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate OnlineClientDataTemplate { get; set; }
        public DataTemplate OfflineClientDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item is OnlineClientInformation ? OnlineClientDataTemplate : OfflineClientDataTemplate;
        }
    }
}