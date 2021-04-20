namespace Orcus.Shared.Commands.UserInteraction
{
    public enum UserInteractionCommunication : byte
    {
        GetWelcomePackage,
        WelcomePackage,
        TextToSpeech,
        SpeakingText,
        SpeakingFinished,
        OpenInEditor,
        OpenedInEditorSuccessfully,
        NotifyIconMessage,
        NotifyIconMessageOpened
    }
}