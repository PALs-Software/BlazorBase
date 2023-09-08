using System;

namespace BlazorBase.CRUD.ModelServiceProviderInjection;

public interface IModeInjectServiceProvider
{
    IServiceProvider ServiceProvider { get; set; }
}
