using System.Collections.Generic;
using Orcus.Shared.Core;

namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Arguments which influence the builder process
    /// </summary>
    public interface IBuilderArguments
    {
        /// <summary>
        ///     The type of the save file dialog
        /// </summary>
        SaveDialogType SaveDialog { get; set; }

        /// <summary>
        ///     The filter of the save file dialog
        /// </summary>
        string SaveDialogFilter { get; set; }

        /// <summary>
        ///     The applied settings
        /// </summary>
        IReadOnlyList<IBuilderProperty> Settings { get; }

        /// <summary>
        ///     Subscribe to a builder event
        /// </summary>
        /// <param name="builderEvent">The event to subscribe to</param>
        void SubscribeBuilderEvent(BuilderEvent builderEvent);
    }
}