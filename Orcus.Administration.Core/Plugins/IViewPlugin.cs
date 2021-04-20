using System;

namespace Orcus.Administration.Core.Plugins
{
    public interface IViewPlugin : IPlugin
    {
        Type CommandView { get; }
        Type ViewType { get; }
    }
}