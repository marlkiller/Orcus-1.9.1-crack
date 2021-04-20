using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OrcusPluginStudio.Views.ManualTest
{
    internal class ViewManager : IValueConverter
    {
        private readonly ReadOnlyDictionary<Type, Type> _viewsViewModels;

        public ViewManager()
        {
            if (_viewsViewModels == null)
                _viewsViewModels =
                    new ReadOnlyDictionary<Type, Type>(
                        new Dictionary<Type, Type>
                        {
                            {typeof (Core.Test.ManualTests.AudioPluginTest), typeof (AudioPluginTest)},
                            {typeof (Core.Test.ManualTests.BuildPluginTest), typeof (BuildPluginTest)},
                            {typeof (Core.Test.ManualTests.ClientPluginTest), typeof (ClientPluginTest)},
                            {typeof (Core.Test.ManualTests.CommandViewTest), typeof (CommandViewTest)}
                        });
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var type = value.GetType();

            if (_viewsViewModels.ContainsKey(type))
                return GetView(type, value, _viewsViewModels[type]);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private FrameworkElement GetView(Type viewModelType, object viewModel, Type viewType)
        {
            var view = (FrameworkElement) Activator.CreateInstance(viewType);
            view.DataContext = viewModel;

            return view;
        }
    }
}