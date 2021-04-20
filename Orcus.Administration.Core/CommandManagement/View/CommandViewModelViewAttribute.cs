using System;
using System.Windows;

namespace Orcus.Administration.Core.CommandManagement.View
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandViewModelViewAttribute : Attribute
    {
        public CommandViewModelViewAttribute(Type viewType)
        {
            if (!viewType.IsSubclassOf(typeof (FrameworkElement)))
                throw new ArgumentException(nameof(viewType));

            ViewType = viewType;
        }

        public Type ViewType { get; }
    }
}