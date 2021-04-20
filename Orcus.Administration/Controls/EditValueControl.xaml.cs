using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for EditValueControl.xaml
    /// </summary>
    public partial class EditValueControl
    {
        public static readonly DependencyProperty UIntValueProperty = DependencyProperty.Register(
            "UIntValue", typeof (uint), typeof (EditValueControl),
            new PropertyMetadata(default(uint), ValuePropertyChangedCallback));

        public static readonly DependencyProperty ULongValueProperty = DependencyProperty.Register(
            "ULongValue", typeof (ulong), typeof (EditValueControl),
            new PropertyMetadata(default(ulong), ValuePropertyChangedCallback));

        public EditValueControl()
        {
            InitializeComponent();
        }

        public ValueSize ValueSize { get; set; }

        public uint UIntValue
        {
            get { return (uint) GetValue(UIntValueProperty); }
            set { SetValue(UIntValueProperty, value); }
        }

        public ulong ULongValue
        {
            get { return (ulong) GetValue(ULongValueProperty); }
            set { SetValue(ULongValueProperty, value); }
        }

        private void RefreshValue()
        {
            if (HexdecimalRadioButton.IsChecked == true)
            {
                var hexString = ValueSize == ValueSize.UInt32 ? UIntValue.ToString("X") : ULongValue.ToString("X");
                if (!string.Equals(hexString, ValueTextBox.Text, StringComparison.OrdinalIgnoreCase))
                    ValueTextBox.Text = hexString;
            }
            else
            {
                ValueTextBox.Text = ValueSize == ValueSize.UInt32 ? UIntValue.ToString() : ULongValue.ToString();
            }
        }

        private static void ValuePropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as EditValueControl;
            control?.RefreshValue();
        }

        private void ValueTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (char.IsDigit(e.Text, e.Text.Length - 1) || (HexdecimalRadioButton.IsChecked == true && IsHex(e.Text)))
                return;

            e.Handled = true;
        }

        private bool IsHex(IEnumerable<char> chars)
        {
            return
                chars.Select(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    .All(isHex => isHex);
        }

        private void ValueTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ValueTextBox.Text))
            {
                SetUInt(0);
                SetULong(0);
                return;
            }

            if (ValueSize == ValueSize.UInt32)
            {
                uint value;
                if (uint.TryParse(ValueTextBox.Text,
                    HexdecimalRadioButton.IsChecked == true
                        ? NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier
                        : NumberStyles.Integer,
                    CultureInfo.CurrentCulture, out value))
                {
                    SetUInt(value);
                }
                else
                {
                    ValueTextBox.Text = "0";
                }
            }
            else
            {
                ulong value;
                if (ulong.TryParse(ValueTextBox.Text,
                    HexdecimalRadioButton.IsChecked == true
                        ? NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier
                        : NumberStyles.Integer,
                    CultureInfo.CurrentCulture, out value))
                {
                    SetULong(value);
                }
                else
                {
                    ValueTextBox.Text = "0";
                }
            }
        }

        private void SetUInt(uint value)
        {
            if (UIntValue != value)
                UIntValue = value;
        }

        private void SetULong(ulong value)
        {
            if (ULongValue != value)
                ULongValue = value;
        }

        private void ValueTextBox_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
/*
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }*/
        }

        private void HexdecimalRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            ValueTextBox.Text = ValueSize == ValueSize.UInt32 ? UIntValue.ToString("X") : ULongValue.ToString("X");
        }

        private void DecimalRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            ValueTextBox.Text = ValueSize == ValueSize.UInt32 ? UIntValue.ToString() : ULongValue.ToString();
        }
    }

    public enum ValueSize
    {
        UInt32,
        UInt64
    }
}