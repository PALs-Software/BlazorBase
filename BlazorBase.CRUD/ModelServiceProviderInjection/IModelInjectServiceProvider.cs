using System;

namespace BlazorBase.CRUD.ModelServiceProviderInjection;

public interface IModelInjectServiceProvider
{
    IServiceProvider ServiceProvider { get; set; }
}
