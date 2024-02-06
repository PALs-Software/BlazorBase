using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.CRUD.ModelServiceProviderInjection;

public class ServiceProviderInterceptor : IMaterializationInterceptor
{
    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        if (instance is IModelInjectServiceProvider entity)
            entity.ServiceProvider = materializationData.Context.GetService<ScopedServiceProvider>().ServiceProvider;

        return instance;
    }
}
