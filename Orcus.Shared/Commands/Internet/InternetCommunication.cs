namespace Orcus.Shared.Commands.Internet
{
    public enum InternetCommunication : byte
    {
        DownloadAndOpenFile,
        StartMassDownload,
        StopMassDownload,
        ResponseDownloadFileError
    }

    public enum MassDownloadServer
    {
        Internode,
        NetCologne,
        SpeedtestX,
        ReliableServers
    }
}