namespace Orcus.Business.Manager.Core
{
    public interface ICurrentStatusReporter
    {
        string CurrentStatus { get; set; }
        double CurrentProgress { get; set; }
    }
}