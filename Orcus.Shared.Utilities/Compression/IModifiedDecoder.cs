namespace Orcus.Shared.Utilities.Compression
{
    public interface IModifiedDecoder : IImageDecoder
    {
        IModifiedDecoder AppendModifier<T>(T writeableBitmapModifierTask) where T : IWriteableBitmapModifierTask;
    }
}