namespace Orcus.Shared.Commands.WindowsCustomizer
{
    public enum WindowsCustomizerCommunication
    {
        GetCurrentSettings,
        ResponseCurrentSettings,
        SetProperty,
        ChangeBooleanValue,
        BooleanValueChanged,
        UnauthorizedAccessException
    }
}