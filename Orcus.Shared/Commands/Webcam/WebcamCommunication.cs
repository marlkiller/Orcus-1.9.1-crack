namespace Orcus.Shared.Commands.Webcam
{
    public enum WebcamCommunication
    {
        GetImage,
        GetWebcams,
        Start,
        Stop,
        ResponseStarted,
        ResponseStopped,
        ResponseFrame,
        ResponseWebcams,
        ResponseResolutionNotFoundUsingDefault,
        ResponseNoFrameReceived,
        ResponseNotSupported
    }
}