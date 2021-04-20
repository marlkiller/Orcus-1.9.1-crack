using System;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace Orcus.Administration.Behavior
{
    internal class AutoScrollBehavior : Behavior<ScrollViewer>
    {
        private double _height;
        private ScrollViewer _scrollViewer;

        protected override void OnAttached()
        {
            base.OnAttached();

            _scrollViewer = AssociatedObject;
            _scrollViewer.LayoutUpdated += _scrollViewer_LayoutUpdated;
        }

        private void _scrollViewer_LayoutUpdated(object sender, EventArgs e)
        {
            if (_scrollViewer.ExtentHeight != _height)
            {
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.ExtentHeight);
                _height = _scrollViewer.ExtentHeight;

                var verticalAnimation = new DoubleAnimation
                {
                    From = _scrollViewer.VerticalOffset,
                    To = _scrollViewer.ScrollableHeight,
                    Duration = TimeSpan.FromMilliseconds(400),
                    EasingFunction = new CircleEase {EasingMode = EasingMode.EaseIn}
                };

                _scrollViewer.BeginAnimation(ScrollAnimationBehaviour.VerticalOffsetProperty, verticalAnimation);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (_scrollViewer != null)
                _scrollViewer.LayoutUpdated -= _scrollViewer_LayoutUpdated;
        }
    }
}