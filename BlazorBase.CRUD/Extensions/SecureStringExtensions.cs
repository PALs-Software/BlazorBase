#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace BlazorBase.CRUD.Extensions
{
    public static class SecureStringExtension
    {
        public static string? ToInsecureString(this SecureString input)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = SecureStringMarshal.SecureStringToGlobalAllocUnicode(input);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        public static string EncryptString(this SecureString input)
        {
            var insecString = ToInsecureString(input) ?? string.Empty;
            if (String.IsNullOrEmpty(insecString))
                return String.Empty;

#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
            byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToInsecureString(input) ?? string.Empty), null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            return Convert.ToBase64String(encryptedData);
        }
    }
}