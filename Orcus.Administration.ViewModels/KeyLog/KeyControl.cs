namespace Orcus.Administration.ViewModels.KeyLog
{
    public abstract class KeyControl
    {
        public bool IsPressed { get; set; }
        public abstract string Text { get; }
    }
}