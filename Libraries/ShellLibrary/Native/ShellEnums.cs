namespace ShellLibrary.Native
{
    /// <summary>
    /// Provides a set of flags to be used with <see cref="Microsoft.WindowsAPICodePack.Shell.SearchCondition"/> 
    /// to indicate the operation in <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory"/> methods.
    /// </summary>
    public enum SearchConditionOperation
    {
        /// <summary>
        /// An implicit comparison between the value of the property and the value of the constant.
        /// </summary>
        Implicit = 0,

        /// <summary>
        /// The value of the property and the value of the constant must be equal.
        /// </summary>
        Equal = 1,

        /// <summary>
        /// The value of the property and the value of the constant must not be equal.
        /// </summary>
        NotEqual = 2,

        /// <summary>
        /// The value of the property must be less than the value of the constant.
        /// </summary>
        LessThan = 3,

        /// <summary>
        /// The value of the property must be greater than the value of the constant.
        /// </summary>
        GreaterThan = 4,

        /// <summary>
        /// The value of the property must be less than or equal to the value of the constant.
        /// </summary>
        LessThanOrEqual = 5,

        /// <summary>
        /// The value of the property must be greater than or equal to the value of the constant.
        /// </summary>
        GreaterThanOrEqual = 6,

        /// <summary>
        /// The value of the property must begin with the value of the constant.
        /// </summary>
        ValueStartsWith = 7,

        /// <summary>
        /// The value of the property must end with the value of the constant.
        /// </summary>
        ValueEndsWith = 8,

        /// <summary>
        /// The value of the property must contain the value of the constant.
        /// </summary>
        ValueContains = 9,

        /// <summary>
        /// The value of the property must not contain the value of the constant.
        /// </summary>
        ValueNotContains = 10,

        /// <summary>
        /// The value of the property must match the value of the constant, where '?' 
        /// matches any single character and '*' matches any sequence of characters.
        /// </summary>
        DosWildcards = 11,

        /// <summary>
        /// The value of the property must contain a word that is the value of the constant.
        /// </summary>
        WordEqual = 12,

        /// <summary>
        /// The value of the property must contain a word that begins with the value of the constant.
        /// </summary>
        WordStartsWith = 13,

        /// <summary>
        /// The application is free to interpret this in any suitable way.
        /// </summary>
        ApplicationSpecific = 14
    }

    /// <summary>
    /// Set of flags to be used with <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory"/>.
    /// </summary>
    public enum SearchConditionType
    {
        /// <summary>
        /// Indicates that the values of the subterms are combined by "AND".
        /// </summary>
        And = 0,

        /// <summary>
        /// Indicates that the values of the subterms are combined by "OR".
        /// </summary>
        Or = 1,

        /// <summary>
        /// Indicates a "NOT" comparison of subterms.
        /// </summary>
        Not = 2,

        /// <summary>
        /// Indicates that the node is a comparison between a property and a 
        /// constant value using a <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionOperation"/>.
        /// </summary>
        Leaf = 3,
    }
}
