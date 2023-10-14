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
    public static IServiceCollection AddBlazorBaseAudioRecorder(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<JSAudioRecorder>();

        return serviceCollection;
    }
}
