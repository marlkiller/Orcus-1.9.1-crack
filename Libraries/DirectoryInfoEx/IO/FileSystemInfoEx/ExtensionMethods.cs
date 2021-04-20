using ShellDll;

namespace System.IO
{
    public static class ExtensionMethods
    {

        public static T RequestPIDL<T>(this FileSystemInfoEx fsi, Func<PIDL, PIDL, T> pidlAndRelPidlFunc)
        {
            PIDL pidl = fsi.getPIDL();
            PIDL relPidl = fsi.getRelPIDL();

            if (pidl == null || relPidl == null)
                return default(T);
            try
            {
                return pidlAndRelPidlFunc(pidl, relPidl);
            }
            finally
            {
                pidl.Free();
                relPidl.Free();
            }
        }

        public static void RequestPIDL(this FileSystemInfoEx fsi, Action<PIDL, PIDL> pidlAndRelPidlFunc)
        {
            PIDL pidl = fsi.getPIDL();
            PIDL relPidl = fsi.getRelPIDL();

            if (pidl == null || relPidl == null)
                return;

            try
            {
                pidlAndRelPidlFunc(pidl, relPidl);
            }
            finally
            {
                pidl.Free();
                relPidl.Free();
            }
        }

        public static T RequestPIDL<T>(this FileSystemInfoEx fsi, Func<PIDL,T> pidlFuncOnly)
        {
            PIDL pidl = fsi.getPIDL();
            if (pidl == null)
                return default(T);

            try
            {
                return pidlFuncOnly(pidl);
            }
            finally
            {
                pidl.Free();
            }
        }

        public static void RequestPIDL(this FileSystemInfoEx fsi, Action<PIDL> pidlFuncOnly)
        {
            PIDL pidl = fsi.getPIDL();
            if (pidl == null)
                return;

            try
            {
                pidlFuncOnly(pidl);
            }
            finally
            {
                pidl.Free();
            }
        }

        public static T RequestRelativePIDL<T>(this FileSystemInfoEx fsi, Func<PIDL, T> relPidlFuncOnly)
        {
            PIDL relPidl = fsi.getRelPIDL();
            if (relPidl == null)
                return default(T);

            try
            {
                return relPidlFuncOnly(relPidl);
            }
            finally
            {
                relPidl.Free();
            }
        }

        public static void RequestRelativePIDL(this FileSystemInfoEx fsi, Action<PIDL> relPidlFuncOnly)
        {
            PIDL relPidl = fsi.getRelPIDL();
            if (relPidl == null)
                return;

            try
            {
                relPidlFuncOnly(relPidl);
            }
            finally
            {
                relPidl.Free();
            }
        }

        public static void RequestPIDL(this FileSystemInfoEx[] fsis, Action<PIDL[], IntPtr[]> pidlFunc)
        {
            PIDL[] pidls = new PIDL[fsis.Length];            
            IntPtr[] ptrs = new IntPtr[fsis.Length];

            for (int i = 0; i < fsis.Length; i++)
            {
                pidls[i] = fsis[i].getPIDL();
                ptrs[i] = pidls[i].Ptr;
            }

            try
            {
                pidlFunc(pidls, ptrs);
            }
            finally
            {
                foreach (PIDL pidl in pidls)
                    pidl.Free();
            }
        }
    }
}
