using System;
using System.Reflection;
using Exceptionless;
using Exceptionless.Plugins;
using Orcus.Shared.DataTransferProtocol;

namespace Orcus.Administration.Core.Exceptionless
{
    public class ServerStackTracePlugin : IEventPlugin
    {
        public void Run(EventPluginContext context)
        {
            if (!context.ContextData.HasException())
                return;

            var exception = context.ContextData.GetException();
            if (exception is ServerException)
            {
                var field = typeof (Exception).GetField("_remoteStackTraceString",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                context.Event.SetProperty("Server Stack Trace", field.GetValue(exception));
            }
        }
    }
}