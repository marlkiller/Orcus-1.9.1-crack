using Exceptionless;
using Exceptionless.Plugins;
using Orcus.Administration.Licensing;

namespace Orcus.Administration.Exceptionless
{
    public class HardwareIdPlugin : IEventPlugin
    {
        public void Run(EventPluginContext context)
        {
            context.Event.SetProperty("Hardware ID", HardwareIdGenerator.HardwareId);
        }
    }
}