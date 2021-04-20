using System;
using System.Windows.Media;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     A new entry to the action list
    /// </summary>
    public interface ICommandView : IDisposable
    {
        /// <summary>
        ///     The name of the menu item
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The category of the menu item
        /// </summary>
        Category Category { get; }

        /// <summary>
        ///     Provides methods to open windows modal to the window the command currently lives in
        /// </summary>
        IWindowService WindowService { get; set; }

        /// <summary>
        ///     Return a small icon which represents this command
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        ///     Constructor of the class. Here, you can store the commands you need in variables (you get them from the
        ///     <see cref="IClientController" />)
        /// </summary>
        /// <param name="clientController">Provides information about the client you can use</param>
        /// <param name="crossViewManager">Provides methods to communicate between different views</param>
        void Initialize(IClientController clientController, ICrossViewManager crossViewManager);

        /// <summary>
        ///     Called when the view is visible for the first time
        /// </summary>
        /// <param name="loadData">
        ///     Determines whether the data should be loaded automatically or the user wants to do that
        ///     manually
        /// </param>
        void LoadView(bool loadData);
    }

    /// <summary>
    ///     The category where the command should be found
    /// </summary>
    public enum Category
    {
        /// <summary>
        ///     The command affects the client
        /// </summary>
        Client,

        /// <summary>
        ///     The command gathers information about the system and displays them to the user
        /// </summary>
        Information,

        /// <summary>
        ///     The command is meant for a funny purpose
        /// </summary>
        Fun,

        /// <summary>
        ///     The command makes changes in the system
        /// </summary>
        System,

        /// <summary>
        ///     The command streams something the user does
        /// </summary>
        Surveillance,

        /// <summary>
        ///     The command gives a utility to the administrator which has nothing specific to do with the system
        /// </summary>
        Utilities
    }
}