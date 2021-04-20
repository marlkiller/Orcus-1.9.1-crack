namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     The type of the save file dialog
    /// </summary>
    public enum SaveDialogType
    {
        /// <summary>
        ///     A standard save file dialog which lets the user define the path and the file name
        /// </summary>
        SaveFileDialog,

        /// <summary>
        ///     A folder browser dialog which only lets the user define the folder but not the file name. The file name will be a
        ///     standard name like 'client.exe'
        /// </summary>
        FolderBrowserDialog
    }
}