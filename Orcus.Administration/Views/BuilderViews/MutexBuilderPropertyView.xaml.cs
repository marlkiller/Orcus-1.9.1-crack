using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for MutexBuilderPropertyView.xaml
    /// </summary>
    public partial class MutexBuilderPropertyView : BuilderPropertyViewUserControl<MutexBuilderProperty>,
        INotifyPropertyChanged
    {
        private RelayCommand _generateMutexCommand;
        private string _mutex;

        public MutexBuilderPropertyView()
        {
            InitializeComponent();
        }

        public string Mutex
        {
            get { return _mutex; }
            set
            {
                if (_mutex != value)
                {
                    _mutex = value;
                    ((MutexBuilderProperty) DataContext).Mutex = value;
                    OnPropertyChanged();
                }
            }
        }


        public RelayCommand GenerateMutexCommand
        {
            get
            {
                return _generateMutexCommand ??
                       (_generateMutexCommand = new RelayCommand(parameter => { Mutex = Guid.NewGuid().ToString("N"); }));
            }
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.GeneralSettings).ComesAfter<ClientTagBuilderProperty>()
            ;

        public override string[] Tags { get; } = {"Mutex", "Id", "Unique", "Instance"};

        protected override void OnCurrentBuilderPropertyChanged(MutexBuilderProperty newValue)
        {
            Mutex = newValue.Mutex;
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            MutexBuilderProperty currentBuilderProperty)
        {
            if (string.IsNullOrWhiteSpace(Mutex))
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorMutex"]);

            return InputValidationResult.Successful;
        }
    }
}