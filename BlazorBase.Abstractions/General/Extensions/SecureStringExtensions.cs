using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlazorBase.Abstractions.General.Extensions;

public static class SecureStringExtension
{
    public static string? ToInsecureString(this SecureString input)
    {
        nint valuePtr = nint.Zero;
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
        var insecureString = input.ToInsecureString();
        if (string.IsNullOrEmpty(insecureString))
            return string.Empty;

        return insecureString.EncryptString();
    }
}