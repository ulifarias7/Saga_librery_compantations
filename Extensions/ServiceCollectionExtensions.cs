using Librery.Saga.Compensations.Abstractions;
using Librery.Saga.Compensations.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Librery.Saga.Compensations.Extensions
{
    //para DI
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSagaOrchestrator(this IServiceCollection services)
        {
            services.AddScoped<ISagaOrchestrator, SagaOrchestrator>();
            return services;
        }
    }
}
