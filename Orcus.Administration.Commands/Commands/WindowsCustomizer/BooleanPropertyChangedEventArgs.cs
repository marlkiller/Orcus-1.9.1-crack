using System;

namespace Orcus.Administration.Commands.WindowsCustomizer
{
    public class BooleanPropertyChangedEventArgs : EventArgs
    {
        public BooleanPropertyChangedEventArgs(string name, bool value)
        {
            Name = name;
            Value = value;
        }

        public bool Value { get; }
        public string Name { get; }
    }
}