using Spike.Hub.Mappers;
using Spike.Hub.Validators;

namespace Spike.Hub;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services
            .AddValidators()
            .AddMappers();

        return services;
    }
    
    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddSingleton<IDtoValidator, DtoValidator>();

        return services;
    } 
    
    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddSingleton<IMapper, Mapper>();

        return services;
    }
}