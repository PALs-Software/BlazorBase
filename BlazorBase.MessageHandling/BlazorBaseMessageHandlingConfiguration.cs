using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.MessageHandling
{
    public static class BlazorBaseMessageHandlingConfiguration
    {
        /// <summary>
        /// Register blazor base message handling and configures the default behaviour.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlazorBaseMessageHandling(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IMessageHandler, MessageHandler>();

            return serviceCollection;
        }
    }
}
