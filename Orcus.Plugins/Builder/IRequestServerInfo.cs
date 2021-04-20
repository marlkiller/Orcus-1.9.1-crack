using System.Collections.Generic;
using Orcus.Shared.Core;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     Can be implemented with a <see cref="IBuilderPropertyView" /> to request some information which are needed for the
    ///     view
    /// </summary>
    public interface IRequestBuilderInfo
    {
        /// <summary>
        ///     Will be set on initialization
        /// </summary>
        IBuilderInfo BuilderInfo { get; set; }
    }

    /// <summary>
    ///     Contains some information from the client builder
    /// </summary>
    public interface IBuilderInfo
    {
        /// <summary>
        ///     The ip addresses the server runs on
        /// </summary>
        List<IpAddressInfo> AvailableIpAddresses { get; }
    }
}