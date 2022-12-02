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
            var insecureString = ToInsecureString(input);
            if (String.IsNullOrEmpty(insecureString))
                return String.Empty;

            return insecureString.EncryptString();
        }
    }
}