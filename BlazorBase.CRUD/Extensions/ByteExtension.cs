using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlazorBase.CRUD.Extensions;

public static class ByteExtension
{
    public static string? DecryptAES(this byte[]? cipherBytes, byte[] key, byte[] iv, Encoding encoding)
    {
        if (cipherBytes == null || cipherBytes.Length <= 0)
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

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new(cipherBytes);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using (StreamReader streamReader = new(cryptoStream, encoding))
        {
            return streamReader.ReadToEnd();
        }
    }

    public static byte[]? DecryptAES(this byte[]? cipherBytes, byte[] key, byte[] iv)
    {
        if (cipherBytes == null || cipherBytes.Length <= 0)
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

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new(cipherBytes);
        using (CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read))
        {
            var dycrypted = new byte[cipherBytes.Length];
            var bytesRead = cryptoStream.Read(dycrypted, 0, cipherBytes.Length);

            return dycrypted.Take(bytesRead).ToArray();
        }
    }

    public static byte[]? EncryptAES(this byte[]? plainBytes, byte[] key, byte[] iv, Encoding encoding)
    {
        if (plainBytes == null || plainBytes.Length <= 0)
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
        using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
        }
      
        return memoryStream.ToArray();
    }
}
