using System;
using System.IO;

namespace Orcus.Server.Core.Connection
{
    public interface IConnection : IDisposable
    {
        BinaryWriter BinaryWriter { get; }
        BinaryReader BinaryReader { get; }
        Stream BaseStream { get; }
        void SetTimeout(int timeout);
    }
}