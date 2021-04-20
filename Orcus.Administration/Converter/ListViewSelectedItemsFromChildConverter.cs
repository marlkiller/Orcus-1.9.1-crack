using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Orcus.Administration.Utilities;

namespace Orcus.Administration.Converter
{
    public class ListViewSelectedItemsFromChildConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as ListBoxItem;
            if (item == null)
                return Binding.DoNothing;

            return item.GetVisualParent<ListBox>().SelectedItems;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}