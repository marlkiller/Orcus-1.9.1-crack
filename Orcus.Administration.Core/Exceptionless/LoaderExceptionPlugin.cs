using System.Linq;
using System.Reflection;
using Exceptionless;
using Exceptionless.Plugins;

namespace Orcus.Administration.Core.Exceptionless
{
    public class LoaderExceptionPlugin : IEventPlugin
    {
        public void Run(EventPluginContext context)
        {
            if (!context.ContextData.HasException())
                return;

            var exception = context.ContextData.GetException();
            var typeLoadException = exception as ReflectionTypeLoadException;
            if (typeLoadException != null)
            {
                var loaderExceptions = typeLoadException.LoaderExceptions;
                context.Event.SetProperty("Loader Exceptions", string.Join("\r\n\r\n", loaderExceptions.Select(x => x.ToString())));
            }
        }
    }
}