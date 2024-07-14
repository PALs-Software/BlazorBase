using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace BlazorBase.Abstractions.General.Extensions;

public static class StringExtension
{
    public static SecureString ToSecureString(this string input)
    {
        var secure = new SecureString();

        foreach (char c in input)
            secure.AppendChar(c);

        secure.MakeReadOnly();
        return secure;
    }

    public static string EncryptString(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
        byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(input), null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
        return Convert.ToBase64String(encryptedData);
    }

    public static SecureString DecryptString(this string? encryptedData)
    {
        try
        {
            if (string.IsNullOrEmpty(encryptedData))
                return new SecureString();

#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
            byte[] decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(encryptedData), null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            return Encoding.Unicode.GetString(decryptedData).ToSecureString();
        }
        catch (Exception)
        {
            return new SecureString();
        }
    }

    public static string? DecryptStringToInsecureString(this string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData))
            return null;

#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
        byte[] decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(encryptedData), null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen

        return Encoding.Unicode.GetString(decryptedData);
    }

    public static string CreateSHA512Hash(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        using var hash = SHA512.Create();
        var hashedInputBytes = hash.ComputeHash(bytes);

        // Convert to text
        // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
        var hashedInputStringBuilder = new StringBuilder(128);
        foreach (var b in hashedInputBytes)
            hashedInputStringBuilder.Append(b.ToString("X2"));
        return hashedInputStringBuilder.ToString();
    }

    #region AES

    public static byte[]? EncryptAES(this string? plainText, byte[] key, byte[] iv, Encoding encoding)
    {
        if (plainText == null || plainText.Length <= 0)
            return null;
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException("IV");
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
        using (StreamWriter streamWriter = new(cryptoStream, encoding))
        {
            streamWriter.Write(plainText);
        }

        return memoryStream.ToArray();
    }

    #endregion
}