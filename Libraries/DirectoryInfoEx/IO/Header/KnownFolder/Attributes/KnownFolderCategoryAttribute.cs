using System;

namespace ShellDll
{
    public class FolderCategoryAttribute : Attribute
    {
        public KnownFolderCategory Category { get; set; }

        public FolderCategoryAttribute(KnownFolderCategory category)
        {
            Category = category;
        }
    }
}
