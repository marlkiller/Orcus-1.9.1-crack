using System;
using System.Collections.ObjectModel;

namespace Orcus.Administration.ViewModels.KeyLog
{
    public class KeyItem
    {
        public KeyItem()
        {
            InlineCollection = new ObservableCollection<object>();
        }

        public string ApplicationName { get; set; }
        public ObservableCollection<object> InlineCollection { get; set; }
        public DateTime Timestamp { get; set; }
    }
}