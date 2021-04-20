using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for ChangeCreationDateBuilderPropertyView.xaml
    /// </summary>
    public partial class ChangeCreationDateBuilderPropertyView :
        BuilderPropertyViewUserControl<ChangeCreationDateBuilderProperty>
    {
        private string _creationDate;

        public ChangeCreationDateBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override string[] Tags { get; } = {"change", "creation", "date", "Erstelldatum", "verändern"};

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation)
                .InGroup(BuilderGroup.Install)
                .ComesAfter<RequireAdministratorPrivilegesInstallerBuilderProperty>();

        public string CreationDate
        {
            get { return _creationDate; }
            set
            {
                DateTime newDateTime;
                if (!DateTime.TryParse(value, out newDateTime))
                    return;

                ((ChangeCreationDateBuilderProperty) DataContext).NewCreationDate = newDateTime;
                _creationDate = newDateTime.ToString(CultureInfo.CurrentCulture);
                OnPropertyChanged();
            }
        }

        protected override void OnCurrentBuilderPropertyChanged(ChangeCreationDateBuilderProperty newValue)
        {
            CreationDate = newValue?.NewCreationDate.ToString(CultureInfo.CurrentCulture);
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            ChangeCreationDateBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}