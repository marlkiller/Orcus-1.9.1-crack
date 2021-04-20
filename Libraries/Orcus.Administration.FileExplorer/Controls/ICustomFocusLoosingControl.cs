using System;

namespace Orcus.Administration.FileExplorer.Controls
{
    public interface ICustomFocusLoosingControl
    {
        event EventHandler FocusLost;
    }
}