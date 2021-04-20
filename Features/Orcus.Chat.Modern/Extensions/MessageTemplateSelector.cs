using System;
using System.Windows;
using System.Windows.Controls;
using Orcus.Chat.Modern.Core;

namespace Orcus.Chat.Modern.Extensions
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MeTemplate { get; set; }
        public DataTemplate YouTemplate { get; set; }
        public Guid MeGuid { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var message = item as Message;
            if (message == null)
                throw new Exception();

            return message.IsFromMe ? MeTemplate : YouTemplate;
        }
    }
}