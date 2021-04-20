namespace System.IO.Tools
{
    public class NullWorkLogger : IWorkLogger
    {
        public void Log(IWork work, logType type, string message)
        {
        }
    }

}
