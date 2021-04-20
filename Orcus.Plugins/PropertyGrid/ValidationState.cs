namespace Orcus.Plugins.PropertyGrid
{
    /// <summary>
    ///     Validation states to tell if the values of the properties are satisfying
    /// </summary>
    public enum ValidationState
    {
        /// <summary>
        ///     There is a problem, the user must change one or more properties in order to continue
        /// </summary>
        Error,

        /// <summary>
        ///     It could be continued, but something seems fishy. The user can decide if he wants to change something or if he
        ///     wants to continue
        /// </summary>
        WarningYesNo,

        /// <summary>
        ///     It can be continued
        /// </summary>
        Success
    }
}