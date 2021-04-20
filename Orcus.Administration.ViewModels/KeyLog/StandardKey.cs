namespace Orcus.Administration.ViewModels.KeyLog
{
    public class StandardKey : KeyControl
    {
        public StandardKey(string text)
        {
            Text = text;
        }

        public override string Text { get; }
    }
}