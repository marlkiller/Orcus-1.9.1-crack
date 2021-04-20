using System;
using System.Runtime.InteropServices;

namespace Orcus.Server.Native
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FileTimeToSystemTime(
            [In] ref long fileTime,
            out SystemTime systemTime);

        [DllImport("AdvApi32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptAcquireContextW(
            out IntPtr providerContext,
            [MarshalAs(UnmanagedType.LPWStr)] string container,
            [MarshalAs(UnmanagedType.LPWStr)] string provider,
            int providerType,
            int flags);

        [DllImport("AdvApi32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptReleaseContext(
            IntPtr providerContext,
            int flags);

        [DllImport("AdvApi32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptGenKey(
            IntPtr providerContext,
            int algorithmId,
            int flags,
            out IntPtr cryptKeyHandle);

        [DllImport("AdvApi32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptDestroyKey(
            IntPtr cryptKeyHandle);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CertStrToNameW(
            int certificateEncodingType,
            IntPtr x500,
            int strType,
            IntPtr reserved,
            [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] encoded,
            ref int encodedLength,
            out IntPtr errorString);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CertCreateSelfSignCertificate(
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
        public static extern bool CertFreeCertificateContext(
            IntPtr certificateContext);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CertOpenStore(
            [MarshalAs(UnmanagedType.LPStr)] string storeProvider,
            int messageAndCertificateEncodingType,
            IntPtr cryptProvHandle,
            int flags,
            IntPtr parameters);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CertCloseStore(
            IntPtr certificateStoreHandle,
            int flags);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CertAddCertificateContextToStore(
            IntPtr certificateStoreHandle,
            IntPtr certificateContext,
            int addDisposition,
            out IntPtr storeContextPtr);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CertSetCertificateContextProperty(
            IntPtr certificateContext,
            int propertyId,
            int flags,
            [In] ref CryptKeyProviderInformation data);

        [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PFXExportCertStoreEx(
            IntPtr certificateStoreHandle,
            ref CryptoApiBlob pfxBlob,
            IntPtr password,
            IntPtr reserved,
            int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr handle);
    }
}