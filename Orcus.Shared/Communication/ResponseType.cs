namespace Orcus.Shared.Communication
{
    /// <summary>
    ///     Token from the client
    /// </summary>
    public enum ResponseType
    {
        CommandResponse = 1,
        CommandNotFound,
        CommandError,
        NewClient,
        StatusUpdate
    }
}