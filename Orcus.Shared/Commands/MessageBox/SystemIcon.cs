using System;

namespace Orcus.Shared.Commands.MessageBox
{
    [Serializable]
    public enum SystemIcon : byte
    {
        Error,
        Info,
        Warning,
        Question,
        None
    }
}