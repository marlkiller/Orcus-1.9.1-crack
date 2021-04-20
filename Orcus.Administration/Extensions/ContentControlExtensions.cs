using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Orcus.Administration.Extensions
{
    public static class ContentControlExtensions
    {
        public static readonly DependencyProperty ContentChangedAnimationProperty = DependencyProperty.RegisterAttached(
            "ContentChangedAnimation", typeof (Storyboard), typeof (ContentControlExtensions),
            new PropertyMetadata(default(Storyboard), ContentChangedAnimationPropertyChangedCallback));

        private static readonly Dictionary<FrameworkElement, object> LastContentDictionary =
            new Dictionary<FrameworkElement, object>();

        public static void SetContentChangedAnimation(DependencyObject element, Storyboard value)
        {
            element.SetValue(ContentChangedAnimationProperty, value);
        }

        public static Storyboard GetContentChangedAnimation(DependencyObject element)
        {
            return (Storyboard) element.GetValue(ContentChangedAnimationProperty);
        }

        private static void ContentChangedAnimationPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var contentControl = dependencyObject as ContentControl;
            if (contentControl == null)
                throw new Exception("Can only be applied to a ContentControl");

            var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty,
                typeof (ContentControl));

            propertyDescriptor.RemoveValueChanged(contentControl, ContentChangedHandler);
            propertyDescriptor.AddValueChanged(contentControl, ContentChangedHandler);
        }

        private static void ContentChangedHandler(object sender, EventArgs eventArgs)
        {
            var animateObject = (ContentControl) sender;
            if (LastContentDictionary.ContainsKey(animateObject) &&
                LastContentDictionary[animateObject] == animateObject.Content)
                return;

            if (!LastContentDictionary.ContainsKey(animateObject))
                LastContentDictionary.Add(animateObject, animateObject.Content);
            else if (animateObject.Content != null)
                LastContentDictionary[animateObject] = animateObject.Content;

            var storyboard = GetContentChangedAnimation(animateObject);
            storyboard.Begin(animateObject);
        }
    }
}