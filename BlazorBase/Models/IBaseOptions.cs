using BlazorBase.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace BlazorBase.Models;
public interface IBaseOptions
{
    BaseOptionsImportMode OptionsImportMode { get; set; }

    void ImportOptions<T>(IServiceProvider serviceProvider, Action<T> action) where T : class, IBaseOptions
    {
        action?.Invoke(this as T);

        if (OptionsImportMode != BaseOptionsImportMode.Database)
            return;

        var dbContext = serviceProvider.GetService<DbContext>();
        if (dbContext == null)
            throw new Exception("No \"DbContext\" is registered in the service provider, but this service is required if you choose the option import mode \"Database\"");

        var entry = dbContext.Set<T>().FirstOrDefault();

        if (entry == null)
            return;

        entry.TransferPropertiesTo(this);
    }
}
