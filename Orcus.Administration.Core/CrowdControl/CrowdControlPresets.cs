using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.Utilities;
using StringExtensions = Orcus.Administration.Core.Utilities.StringExtensions;

namespace Orcus.Administration.Core.CrowdControl
{
    public class CrowdControlPresets
    {
        private static CrowdControlPresets _current;
        private string _folderPath;
        private readonly Dictionary<PresetInfo, string> _presetPaths;

        private CrowdControlPresets()
        {
            Presets = new ObservableCollection<PresetInfo>();
            _presetPaths = new Dictionary<PresetInfo, string>();
        }

        public static CrowdControlPresets Current => _current ?? (_current = new CrowdControlPresets());

        public ObservableCollection<PresetInfo> Presets { get; set; }

        public void Load(string folderPath)
        {
            _folderPath = folderPath;

            var folder = new DirectoryInfo(folderPath);
            if (folder.Exists)
            {
                var xmlSerializer = new XmlSerializer(typeof(PresetInfo), GetNeededTypes());
                foreach (var fileInfo in folder.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        PresetInfo presetInfo;
                        using (var streamReader = new StreamReader(fileInfo.FullName))
                            presetInfo = (PresetInfo) xmlSerializer.Deserialize(streamReader);

                        Presets.Add(presetInfo);
                        _presetPaths.Add(presetInfo, fileInfo.FullName);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private static Type[] GetNeededTypes()
        {
            var result = new List<Type>(DynamicCommandInfo.RequiredTypes);
            result.AddRange(StaticCommander.StaticCommands.Select(x => x.Key));
            return result.ToArray();
        }

        public void AddPreset(PresetInfo presetInfo)
        {
            var path =
                FileExtensions.MakeUnique(Path.Combine(_folderPath,
                    StringExtensions.RemoveSpecialCharacters(presetInfo.Name.Replace(" ", "")) + ".xml"));

            Directory.CreateDirectory(_folderPath);
            var xmlSerializer = new XmlSerializer(typeof(PresetInfo), GetNeededTypes());
            using (var streamWriter = new StreamWriter(path))
                xmlSerializer.Serialize(streamWriter, presetInfo);

            Presets.Add(presetInfo);
            _presetPaths.Add(presetInfo, path);
        }

        public void RemovePreset(PresetInfo presetInfo)
        {
            Presets.Remove(presetInfo);
            var path = _presetPaths[presetInfo];
            File.Delete(path);
            _presetPaths.Remove(presetInfo);
        }
    }
}