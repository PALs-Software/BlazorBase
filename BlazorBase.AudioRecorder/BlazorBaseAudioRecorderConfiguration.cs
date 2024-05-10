using BlazorBase.AudioRecorder.Components;
using BlazorBase.AudioRecorder.Models;
using BlazorBase.AudioRecorder.Services;
using BlazorBase.CRUD.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.AudioRecorder;
public static class BlazorBaseAudioRecorderConfiguration
{
    /// <summary>
    /// Register blazor base audio recorder and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseAudioRecorder<TBaseRecordImplementation>(this IServiceCollection serviceCollection) where TBaseRecordImplementation : class, IBaseAudioRecord
    {
        serviceCollection
            .AddSingleton<AudioConverter>()
            .AddScoped<JSAudioRecorder>()
            .AddScoped<JSRawAudioRecorder>()
            .AddTransient<IBaseAudioRecord, TBaseRecordImplementation>()
            .AddTransient<IBasePropertyCardInput, BaseAudioRecordInput>()
            .AddTransient<IBasePropertyListPartInput, BaseAudioRecordListPartInput>()
            .AddTransient<IBasePropertyListDisplay, BaseAudioRecordListDisplay>();

        return serviceCollection;
    }

    public static IServiceCollection AddBlazorBaseAudioRecorderWithoutCRUDSupport(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<AudioConverter>()
            .AddScoped<JSAudioRecorder>()
            .AddScoped<JSRawAudioRecorder>();

        return serviceCollection;
    }
}
