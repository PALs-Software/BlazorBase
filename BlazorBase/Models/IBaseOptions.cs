using BlazorBase.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorBase.Models;
public interface IBaseOptions
{
    BaseOptionsImportMode OptionsImportMode { get; set; }
    Type OptionsImportFromDatabaseEntryType { get; set; }

    void ImportOptions<T>(IServiceProvider serviceProvider, Action<T> action) where T : class, IBaseOptions
    {
        action?.Invoke(this as T);

        if (OptionsImportMode != BaseOptionsImportMode.Database)
            return;

        var dbContext = serviceProvider.GetService<DbContext>();
        if (dbContext == null)
            throw new Exception("No \"DbContext\" is registered in the service provider, but this service is required if you choose the option import mode \"Database\"");

        if (OptionsImportFromDatabaseEntryType == null)
            throw new Exception("If the option import mode is \"Database\" you must choose the database entry type where the data will be loaded from");

        var entry = dbContext.Find(OptionsImportFromDatabaseEntryType);

        if (entry == null)
            entry = Activator.CreateInstance(OptionsImportFromDatabaseEntryType);

        entry.TransferPropertiesTo(this);
    }
}
