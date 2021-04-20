namespace Orcus.Native.Shell
{
    internal enum HResult
    {
        Ok = 0x0000,
        False = 0x0001,
        InvalidArguments = unchecked((int) 0x80070057),
        OutOfMemory = unchecked((int) 0x8007000E),
        NoInterface = unchecked((int) 0x80004002),
        Fail = unchecked((int) 0x80004005),
        ElementNotFound = unchecked((int) 0x80070490),
        TypeElementNotFound = unchecked((int) 0x8002802B),
        NoObject = unchecked((int) 0x800401E5),
        Win32ErrorCanceled = 1223,
        Canceled = unchecked((int) 0x800704C7),
        ResourceInUse = unchecked((int) 0x800700AA),
        AccessDenied = unchecked((int) 0x80030005)
    }
}