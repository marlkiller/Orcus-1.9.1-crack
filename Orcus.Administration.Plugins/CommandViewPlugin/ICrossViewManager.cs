using System;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Methods to communicate between views
    /// </summary>
    public interface ICrossViewManager
    {
        /// <summary>
        ///     Determines whether the method exists
        /// </summary>
        /// <param name="methodGuid">The guid of that method</param>
        /// <returns>Returns if that method exists</returns>
        bool ContainsMethod(Guid methodGuid);

        /// <summary>
        ///     Register a new method
        /// </summary>
        /// <typeparam name="T">The type of the parameter</typeparam>
        /// <param name="commandView">The command view of that method</param>
        /// <param name="methodGuid">The guid of the method</param>
        /// <param name="eventHandler">The handler is executed when the method gets called</param>
        void RegisterMethod<T>(ICommandView commandView, Guid methodGuid, EventHandler<T> eventHandler);

        /// <summary>
        ///     Execute a method
        /// </summary>
        /// <param name="methodGuid">The guid of the method</param>
        /// <param name="parameter">The parameter of the method</param>
        void ExecuteMethod(Guid methodGuid, object parameter);
    }

    /// <summary>
    ///     Some basic cross view command which can be used in Orcus
    /// </summary>
    public static class CrossViewManagerExtensions
    {
        private static readonly Guid FileExplorerOpenPathMethodGuid = new Guid(0x417b8c5a, 0xe218, 0x0145, 0x9c, 0xfe,
            0xff, 0xc7, 0x02, 0xc6, 0x61, 0x48);

        private static readonly Guid ConsoleOpenCommandPrompt = new Guid(0xa8644e87, 0x2509, 0xa247, 0x9f, 0x5f, 0x2d,
            0xa4, 0xbc, 0x70, 0xc9, 0x89);

        private static readonly Guid WindowManagerShowProcessWindows = new Guid(0xcf23f35b, 0x9e90, 0x634b, 0xbb, 0x60,
            0x34, 0xcb, 0xb8, 0x78, 0x7c, 0x2c);

        /// <summary>
        ///     Open the given path in the file explorer of Orcus
        /// </summary>
        /// <param name="crossViewManager">The current cross view manager</param>
        /// <param name="path">The path which should be opened</param>
        public static void OpenPathInFileExplorer(this ICrossViewManager crossViewManager, string path)
        {
            crossViewManager.ExecuteMethod(FileExplorerOpenPathMethodGuid, path);
        }

        /// <summary>
        ///     Open a command promopt (console) with the given path as current directory
        /// </summary>
        /// <param name="crossViewManager">The current cross view manager</param>
        /// <param name="path">The current directory of the console</param>
        public static void OpenCommandPrompt(this ICrossViewManager crossViewManager, string path)
        {
            crossViewManager.ExecuteMethod(ConsoleOpenCommandPrompt, path);
        }

        /// <summary>
        ///     Open the window manager which is filtered by the given process id
        /// </summary>
        /// <param name="crossViewManager">The current cross view manager</param>
        /// <param name="processId">The id of the process which windows should be visible</param>
        public static void OpenWindowManagerWithProcessId(this ICrossViewManager crossViewManager, int processId)
        {
            crossViewManager.ExecuteMethod(WindowManagerShowProcessWindows, processId);
        }
    }
}