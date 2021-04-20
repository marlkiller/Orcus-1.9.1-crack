using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Views.Licensing.Pages;
using Orcus.Administration.Views.Licensing.Steps;

namespace Orcus.Administration.Views.Licensing
{
    public class ViewManager : IValueConverter
    {
        private static ReadOnlyDictionary<Type, Type> _viewsViewModels;
        private readonly Dictionary<Type, FrameworkElement> _cachedViews;

        public ViewManager()
        {
            _cachedViews = new Dictionary<Type, FrameworkElement>();

            if (_viewsViewModels == null)
                _viewsViewModels = new ReadOnlyDictionary<Type, Type>(new Dictionary<Type, Type>
                {
                    {typeof (Step1), typeof (Page1)},
                    {typeof (Step2), typeof (Page2)},
                    {typeof (Step3), typeof (Page3)},
                    {typeof (Step4), typeof (Page4)},
                    {typeof (Step5), typeof (Page5)},
                    {typeof (StepFinished), typeof (PageFinished)},
                    {typeof (StepError), typeof (PageError)}
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
            if (_cachedViews.ContainsKey(viewModelType))
            {
                var result = _cachedViews[viewModelType];
                result.DataContext = viewModel;
                return result;
            }

            var view = (FrameworkElement) Activator.CreateInstance(viewType);
            view.DataContext = viewModel;

            _cachedViews.Add(viewModelType, view);
            return view;
        }
    }
}