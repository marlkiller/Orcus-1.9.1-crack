using System;

namespace Orcus.Administration.Core.Plugins
{
    internal interface ICommandPlugin
    {
        Type CommandType { get; }
        uint CommandId { get; }
    }
}