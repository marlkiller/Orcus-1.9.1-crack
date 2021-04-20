using System.IO;
using ShellLibrary.Native;

namespace ShellLibrary
{
    /// <summary>
    ///     A file in the Shell Namespace
    /// </summary>
    public class ShellFile : ShellObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal ShellFile(string path)
        {
            // Get the absolute path
            string absPath = ShellHelper.GetAbsolutePath(path);

            // Make sure this is valid
            if (!File.Exists(absPath))
            {
                throw new FileNotFoundException(
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "The given path does not exist ({0})", path));
            }

            ParsingName = absPath;
        }

        internal ShellFile(IShellItem2 shellItem)
        {
            nativeShellItem = shellItem;
        }

        /// <summary>
        ///     The path for this file
        /// </summary>
        virtual public string Path
        {
            get { return ParsingName; }
        }

        /// <summary>
        ///     Constructs a new ShellFile object given a file path
        /// </summary>
        /// <param name="path">The file or folder path</param>
        /// <returns>ShellFile object created using given file path.</returns>
        static public ShellFile FromFilePath(string path)
        {
            return new ShellFile(path);
        }
    }
}