using Deepgram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using Spike.Application.Clients;
using Spike.Application.Repositories;
using Spike.Infrastructure.LLM;
using Spike.Infrastructure.Repositories;
using Spike.Infrastructure.Speech;

namespace Spike.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRepositories()
            .AddDeepgram()
            .AddElevenLabs(configuration)
            .AddOpenAi(configuration);

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ICallRepository, InMemoryCallRepository>();
        services.AddSingleton<IPromptsRepository, FilePromptsRepository>();

        return services;
    }

    private static IServiceCollection AddDeepgram(this IServiceCollection services)
    {
        var deepgramRestClient = ClientFactory.CreateListenRESTClient();
        services.AddSingleton(deepgramRestClient);

        services.AddScoped<ISpeechToTextClient, DeepgramClient>();

        return services;
    }

    private static IServiceCollection AddElevenLabs(this IServiceCollection services, IConfiguration configuration)
    {
        var elevenLabsClient = new ElevenLabs.ElevenLabsClient();
        services.AddSingleton(elevenLabsClient);
        
        services.AddScoped<ITextToSpeechClient, ElevenLabsClient>();

        return services;
    }

    private static IServiceCollection AddOpenAi(this IServiceCollection services, IConfiguration configuration)
    {
        var openAiClient = new ChatClient("gpt-4.1", configuration["OPENAI_API_KEY"]);
        services.AddSingleton(openAiClient);

        services.AddScoped<IChatClient, OpenAiClient>();

        return services;
    }
}