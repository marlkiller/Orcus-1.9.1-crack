namespace Orcus.Shared.Commands.TextChat
{
    public enum TextChatCommunication : byte
    {
        ChatOpened,
        ChatClosed,
        ResponseMessage,
        SendMessage,
        OpenChat,
        Close,
        InitializationFailed
    }
}