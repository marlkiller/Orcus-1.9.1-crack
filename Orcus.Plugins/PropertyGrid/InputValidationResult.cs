using Orcus.Shared.Resharper;

namespace Orcus.Plugins.PropertyGrid
{
    /// <summary>
    ///     The result of the validation process
    /// </summary>
    public class InputValidationResult
    {
        /// <summary>
        ///     The validation was successful
        /// </summary>
        public static InputValidationResult Successful = new InputValidationResult(null, ValidationState.Success);

        /// <summary>
        ///     Initialize a new <see cref="InputValidationResult" />
        /// </summary>
        /// <param name="message">The message which should be displayed</param>
        /// <param name="validationState">The validation state</param>
        public InputValidationResult([LocalizationRequired] string message, ValidationState validationState)
        {
            Message = message;
            ValidationState = validationState;
        }

        /// <summary>
        ///     The message
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     The validation state
        /// </summary>
        public ValidationState ValidationState { get; }

        /// <summary>
        ///     The validation failed
        /// </summary>
        /// <param name="message">The error message</param>
        /// <returns></returns>
        public static InputValidationResult Error([LocalizationRequired] string message)
        {
            return new InputValidationResult(message, ValidationState.Error);
        }
    }
}