namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     Interface which should be implemented when the builder property is the leader of the <see cref="BuilderGroup" />
    /// </summary>
    public interface ILeaderBuilderPropertyView : IBuilderPropertyView
    {
        /// <summary>
        ///     Set to true if the childs should be enabled
        /// </summary>
        bool EnableSubSettings { get; }
    }
}