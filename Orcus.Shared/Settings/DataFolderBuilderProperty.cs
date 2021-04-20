using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class DataFolderBuilderProperty : IBuilderProperty
    {
        public string Path { get; set; } = @"%appdata%\Orcus";

        public IBuilderProperty Clone()
        {
            return new DataFolderBuilderProperty {Path = Path};
        }
    }
}