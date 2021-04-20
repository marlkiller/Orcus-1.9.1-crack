using System;

// ReSharper disable InconsistentNaming

namespace Orcus.Shared.Commands.MessageBox
{
    /// <summary>Specifies constants defining which buttons to display on a <see cref="T:System.Windows.Forms.MessageBox" />.</summary>
    /// <filterpriority>2</filterpriority>
    [Serializable]
    public enum MessageBoxButtons
    {
        /// <summary>The message box contains an OK button.</summary>
        OK,

        /// <summary>The message box contains OK and Cancel buttons.</summary>
        OKCancel,

        /// <summary>The message box contains Abort, Retry, and Ignore buttons.</summary>
        AbortRetryIgnore,

        /// <summary>The message box contains Yes, No, and Cancel buttons.</summary>
        YesNoCancel,

        /// <summary>The message box contains Yes and No buttons.</summary>
        YesNo,

        /// <summary>The message box contains Retry and Cancel buttons.</summary>
        RetryCancel
    }
}