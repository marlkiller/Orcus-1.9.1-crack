namespace Orcus.Shared.Communication
{
    public enum AuthentificationFeedback
    {
        GetKey,
        InvalidKey,
        InvalidApiVersion,
        Accepted,
        ApiVersionOkayGetPassword,
        InvalidPassword,
        GetHardwareId,
        GetClientTag,
        ServerIsFull,
        ApiVersionOkayWantANamedPipe
    }
}