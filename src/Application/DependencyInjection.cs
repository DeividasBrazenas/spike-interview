using Microsoft.Extensions.DependencyInjection;
using Spike.Application.Services;
using Spike.Application.Services.Abstractions;

namespace Spike.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICallService, CallService>();
        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}