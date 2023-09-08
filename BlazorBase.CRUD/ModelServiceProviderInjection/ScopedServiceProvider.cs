using System;

namespace BlazorBase.CRUD.ModelServiceProviderInjection;

public class ScopedServiceProvider
{
    public IServiceProvider ServiceProvider { get; private set; }

    public ScopedServiceProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
