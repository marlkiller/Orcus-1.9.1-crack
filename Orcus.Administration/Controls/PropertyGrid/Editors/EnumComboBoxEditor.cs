using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class EnumComboBoxEditor : ComboBoxEditor
    {
        protected override IEnumerable CreateItemsSource()
        {
            var enumValues = Enum.GetValues(PropertyItem.Property.PropertyType).Cast<object>().Distinct();

            return (
                from object enumValue in enumValues
                select new EnumerationMember
                {
                    Value = enumValue,
                    Description = GetDescription(enumValue)
                }).ToArray();
        }

        protected override void InitializeControl()
        {
            base.InitializeControl();

            Editor.SelectedValuePath = "Value";
            Editor.DisplayMemberPath = "Description";
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = PropertyItem.Property.PropertyType
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof (DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;


            return descriptionAttribute != null
                ? descriptionAttribute.Description
                : enumValue.ToString();
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }
}