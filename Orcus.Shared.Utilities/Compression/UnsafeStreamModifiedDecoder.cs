using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Orcus.Shared.Utilities.Compression
{
    internal unsafe class UnsafeStreamModifiedDecoder : IModifiedDecoder
    {
        private readonly DecodeDataDelegate _decodeDataDelegate;
        private readonly List<IWriteableBitmapModifierTask> _modifierTasks;

        internal delegate WriteableBitmap DecodeDataDelegate(
            byte* codecBuffer, uint length, Dispatcher dispatcher,
            IEnumerable<IWriteableBitmapModifierTask> modifierTasks);

        public UnsafeStreamModifiedDecoder(IWriteableBitmapModifierTask modifierTask, DecodeDataDelegate decodeDataDelegate)
        {
            _decodeDataDelegate = decodeDataDelegate;
            _modifierTasks = new List<IWriteableBitmapModifierTask> {modifierTask};
        }

        public IModifiedDecoder AppendModifier<T>(T writeableBitmapModifierTask) where T : IWriteableBitmapModifierTask
        {
            _modifierTasks.Add(writeableBitmapModifierTask);
            return this;
        }

        public WriteableBitmap DecodeData(byte* codecBuffer, uint length, Dispatcher dispatcher)
        {
            return _decodeDataDelegate(codecBuffer, length, dispatcher, _modifierTasks);
        }
    }
}