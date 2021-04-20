using System;
using System.Runtime.InteropServices;

namespace Orcus.Administration.Core.Native
{
    internal static class NativeMethods
    {
        /// <summary>
        ///     Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        internal static extern int DeleteObject(IntPtr o);

        [DllImport("kernel32")]
        internal static extern IntPtr BeginUpdateResource(string fileName,
            [MarshalAs(UnmanagedType.Bool)] bool deleteExistingResources);

        [DllImport("kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateResource(IntPtr hUpdate, IntPtr type, IntPtr name, short language,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] data, int dataSize);

        [DllImport("kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EndUpdateResource(IntPtr hUpdate, [MarshalAs(UnmanagedType.Bool)] bool discard);

        [DllImport("AdvApi32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptAcquireContextW(
            out IntPtr providerContext,
            [MarshalAs(UnmanagedType.LPWStr)] string container,
            [MarshalAs(UnmanagedType.LPWStr)] string provider,
            int providerType,
            int flags);

        [DllImport("AdvApi32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptReleaseContext(
            IntPtr providerContext,
            int flags);

        [DllImport(@"Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool CryptGenKey(
            [In] IntPtr hProv,
            [In] uint Algid,
            [In] uint dwFlags,
            [Out] out IntPtr phKey
            );

        [DllImport("AdvApi32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptDestroyKey(
            IntPtr cryptKeyHandle);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertStrToNameW(
            int certificateEncodingType,
            IntPtr x500,
            int strType,
            IntPtr reserved,
            [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] encoded,
            ref int encodedLength,
            out IntPtr errorString);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr CertCreateSelfSignCertificate(
            IntPtr providerHandle,
            [In] ref CryptoApiBlob subjectIssuerBlob,
            int flags,
            [In] ref CryptKeyProviderInformation keyProviderInformation,
            IntPtr signatureAlgorithm,
            [In] ref SystemTime startTime,
            [In] ref SystemTime endTime,
            IntPtr extensions);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertFreeCertificateContext(
            IntPtr certificateContext);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr CertOpenStore(
            [MarshalAs(UnmanagedType.LPStr)] string storeProvider,
            int messageAndCertificateEncodingType,
            IntPtr cryptProvHandle,
            int flags,
            IntPtr parameters);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertCloseStore(
            IntPtr certificateStoreHandle,
            int flags);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertAddCertificateContextToStore(
            IntPtr certificateStoreHandle,
            IntPtr certificateContext,
            int addDisposition,
            out IntPtr storeContextPtr);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertSetCertificateContextProperty(
            IntPtr certificateContext,
            int propertyId,
            int flags,
            [In] ref CryptKeyProviderInformation data);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PFXExportCertStoreEx(
            IntPtr certificateStoreHandle,
            ref CryptoApiBlob pfxBlob,
            IntPtr password,
            IntPtr reserved,
            int flags);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FileTimeToSystemTime(
            [In] ref long fileTime,
            out SystemTime systemTime);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        internal static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass,
            string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
    }
}