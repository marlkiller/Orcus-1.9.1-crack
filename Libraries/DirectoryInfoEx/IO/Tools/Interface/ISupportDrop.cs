///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// LYCJ (c) 2009 - http://www.quickzip.org/components                                                            //
// Release under LGPL license.                                                                                   //
//                                                                                                               //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.IO.Tools
{
    public interface ISupportDrop
    {
        DirectoryInfoEx SelectedDirectory { get; }
        void Refresh();
    }
}
