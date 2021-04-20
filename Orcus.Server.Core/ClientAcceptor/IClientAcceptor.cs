using System.IO;
using System.Net.Security;
using Orcus.Server.Core.Database;

namespace Orcus.Server.Core.ClientAcceptor
{
    interface IClientAcceptor
    {
        int ApiVersion { get; }

        bool LogIn(SslStream sslStream, BinaryReader binaryReader, BinaryWriter binaryWriter, out ClientData clientData,
            out CoreClientInformation basicComputerInformation, out bool isNewClient);
    }
}