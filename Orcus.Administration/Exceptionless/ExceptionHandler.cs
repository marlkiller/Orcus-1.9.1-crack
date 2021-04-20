using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Exceptionless;
using Exceptionless.Plugins.Default;
using NLog;
using Orcus.Administration.Core.Exceptionless;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Administration.Views;

namespace Orcus.Administration.Exceptionless
{
    public class ExceptionHandler
    {
        private bool _handlingException;

        public void Register()
        {
            ExceptionlessClient.Default.Configuration.AddPlugin<SetEnvironmentUserPlugin>();
            ExceptionlessClient.Default.Configuration.AddPlugin<HardwareIdPlugin>();
            ExceptionlessClient.Default.Configuration.AddPlugin<ServerStackTracePlugin>();
            ExceptionlessClient.Default.Configuration.AddPlugin<LoaderExceptionPlugin>();
            ExceptionlessClient.Default.Startup();

            if (ExceptionlessClient.Default.Configuration.SessionsEnabled)
                ExceptionlessClient.Default.SubmitSessionStart();

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            LogManager.GetCurrentClassLogger().Warn(e.Exception, "Unhandled exception caught");
            ApplicationInterface.ForceShutdown = true;
            if (Application.Current == null || _handlingException)
            {
                e.Exception.ToExceptionless()
                    .SetMessage(
                        $"Automatically send because {(Application.Current == null ? "Application.Current = null" : "_handlingException = true")}")
                    .Submit();
                ExceptionlessClient.Default.ProcessQueue();
                return;
            }

            _handlingException = true;
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            foreach (var window in Application.Current.Windows.OfType<Window>())
                try
                {
                    window.Close();
                }
                catch (Exception)
                {
                    // ignored
                }

            new ExceptionWindow(e.Exception).Show();
        }
    }
}