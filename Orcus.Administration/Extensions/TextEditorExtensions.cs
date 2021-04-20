using System;
using System.Windows;
using System.Windows.Data;
using ICSharpCode.AvalonEdit;

namespace Orcus.Administration.Extensions
{
    public class TextEditorExtensions
    {
        public static readonly DependencyProperty BindableTextProperty = DependencyProperty.RegisterAttached(
            "BindableText", typeof (string), typeof (TextEditorExtensions),
            new FrameworkPropertyMetadata(default(string), PropertyChangedCallback)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public static readonly DependencyProperty IsBindableTextEnabledProperty = DependencyProperty.RegisterAttached(
            "IsBindableTextEnabled", typeof (bool), typeof (TextEditorExtensions),
            new PropertyMetadata(default(bool), IsBindbaleTextEnabledPropertyChangedCallback));

        private static void IsBindbaleTextEnabledPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textEditor = dependencyObject as TextEditor;
            if (textEditor == null)
                return;

            textEditor.TextChanged -= TextEditor_TextChanged;
            textEditor.TextChanged += TextEditor_TextChanged;
        }

        public static void SetIsBindableTextEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsBindableTextEnabledProperty, value);
        }

        public static bool GetIsBindableTextEnabled(DependencyObject element)
        {
            return (bool) element.GetValue(IsBindableTextEnabledProperty);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textEditor = (TextEditor) dependencyObject;
            var newValue = (string) dependencyPropertyChangedEventArgs.NewValue;
            if (textEditor.Text != newValue)
                textEditor.Text = newValue;
        }

        private static void TextEditor_TextChanged(object sender, EventArgs e)
        {
            var textEditor = (TextEditor) sender;
            SetBindableText(textEditor, textEditor.Text);
        }

        public static void SetBindableText(DependencyObject element, string value)
        {
            element.SetValue(BindableTextProperty, value);
            var textEditor = (TextEditor) element;
            if (textEditor.Text != value)
                textEditor.Text = value;
        }

        public static string GetBindableText(DependencyObject element)
        {
            return (string) element.GetValue(BindableTextProperty);
        }
    }
}