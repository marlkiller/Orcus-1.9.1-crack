using System;
using Orcus.Administration.Plugins.Administration;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     A plugin just with a view
    /// </summary>
    public interface IViewPlugin
    {
        /// <summary>
        ///     The viewmodel
        /// </summary>
        Type CommandView { get; }

        /// <summary>
        ///     The view
        /// </summary>
        Type View { get; }

        /// <summary>
        ///     Allows the plugin to make modifcations to the administration UI and adding a data viewer
        /// </summary>
        /// <param name="uiModifier">Provides all methods to modify the UI</param>
        void Initialize(IUiModifier uiModifier);
    }
}