namespace Spike.Application.Repositories;

public interface IPromptsRepository
{
    Task<string> GetPromptAsync(string promptId, Dictionary<string, string>? parameters, CancellationToken cancellationToken);
}