using Microsoft.AspNetCore.DataProtection;
using System;

namespace BlazorBase.CRUD.Services;

public static class BlazorBaseDataProtection
{
    public const string ProtectorPurpose = "BlazorBase.CRUD.StringEncryption.v1";

    private static IDataProtector? Protector;

    public static bool IsInitialized => Protector is not null;

    public static void Initialize(IDataProtectionProvider provider)
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));

        Protector = provider.CreateProtector(ProtectorPurpose);
    }

    public static string Protect(string plainText)
    {
        if (Protector is null)
            throw new InvalidOperationException("BlazorBaseDataProtection has not been initialized. Call IApplicationBuilder.UseBlazorBaseCRUD() on application startup.");

        return Protector.Protect(plainText);
    }

    public static string Unprotect(string protectedText)
    {
        if (Protector is null)
            throw new InvalidOperationException("BlazorBaseDataProtection has not been initialized. Call IApplicationBuilder.UseBlazorBaseCRUD() on application startup.");

        return Protector.Unprotect(protectedText);
    }
}
