using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Spike.Application.Repositories;

namespace Spike.Infrastructure.Repositories;

internal class FilePromptsRepository : IPromptsRepository
{
    private readonly IMemoryCache _promptsCache;
    private readonly TimeSpan _promptCacheExpiration = TimeSpan.FromMinutes(10);
    
    private readonly string _templatesDirectory = Path.Combine(AppContext.BaseDirectory, "Repositories", "Prompts");

    public FilePromptsRepository()
    {
        _promptsCache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<string> GetPromptAsync(string promptId, Dictionary<string, string>? parameters, CancellationToken cancellationToken)
    {
        var prompt = await _promptsCache.GetOrCreateAsync<string>(promptId, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _promptCacheExpiration;

            var filePath = Path.Combine(_templatesDirectory, $"{promptId}.txt");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Prompt template '{promptId}' not found at {filePath}");
            }

            return await File.ReadAllTextAsync(filePath, cancellationToken);
        });

        return ApplyParameters(prompt!, parameters);
    }

    private string ApplyParameters(string template, Dictionary<string, string>? parameters)
    {
        if (parameters == null || !parameters.Any())
        {
            return template;
        }

        var sb = new StringBuilder(template);

        foreach (var parameter in parameters)
        {
            sb.Replace($"{{{parameter.Key}}}", parameter.Value);
        }

        return sb.ToString();
    }
}