using System;
using System.IO;

namespace Orcus.Administration.Core.Connection
{
    public interface IConnection : IDisposable
    {
        BinaryReader BinaryReader { get; }
        BinaryWriter BinaryWriter { get; }
        Stream BaseStream { get; }
    }
}