namespace Orcus.Shared.Core
{
    /// <summary>
    ///     A builder property
    /// </summary>
    public interface IBuilderProperty
    {
        /// <summary>
        ///     Clone this builder property with all properties
        /// </summary>
        /// <returns>Return an exact copy of this object</returns>
        IBuilderProperty Clone();
    }
}