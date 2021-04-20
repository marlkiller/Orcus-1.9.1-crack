using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.Behavior
{
    public class TextBlockAutoToolTipBehavior : Behavior<TextBlock>
    {
        private ToolTip _toolTip;

        protected override void OnAttached()
        {
            base.OnAttached();
            _toolTip = new ToolTip
            {
                Placement = PlacementMode.Relative,
                VerticalOffset = 0,
                HorizontalOffset = 0
            };

            ToolTipService.SetShowDuration(_toolTip, int.MaxValue);

            _toolTip.SetBinding(ContentControl.ContentProperty, new Binding
            {
                Path = new PropertyPath("Text"),
                Source = AssociatedObject
            });

            AssociatedObject.TextTrimming = TextTrimming.CharacterEllipsis;
            AssociatedObject.AddValueChanged(TextBlock.TextProperty, TextBlockOnTextChanged);
            AssociatedObject.SizeChanged += AssociatedObjectOnSizeChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveValueChanged(TextBlock.TextProperty, TextBlockOnTextChanged);
            AssociatedObject.SizeChanged -= AssociatedObjectOnSizeChanged;
        }

        private void AssociatedObjectOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            CheckToolTipVisibility();
        }

        private void TextBlockOnTextChanged(object sender, EventArgs eventArgs)
        {
            CheckToolTipVisibility();
        }

        private void CheckToolTipVisibility()
        {
            if (AssociatedObject.ActualWidth == 0)
                Dispatcher.BeginInvoke(
                    new Action(
                        () => AssociatedObject.ToolTip = CalculateIsTextTrimmed(AssociatedObject) ? _toolTip : null),
                    DispatcherPriority.Loaded);
            else
                AssociatedObject.ToolTip = CalculateIsTextTrimmed(AssociatedObject) ? _toolTip : null;
        }

        //Source: https://stackoverflow.com/questions/1041820/how-can-i-determine-if-my-textblock-text-is-being-trimmed
        private static bool CalculateIsTextTrimmed(TextBlock textBlock)
        {
            Typeface typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            FormattedText formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground) {MaxTextWidth = textBlock.ActualWidth};


            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            // The width check detects if any single line is too long to fit within the text area, 
            // this can only happen if there is a long span of text with no spaces.
            return (formattedText.Height > textBlock.ActualHeight || formattedText.MinWidth > formattedText.MaxTextWidth);
        }
    }
}