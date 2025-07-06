using Spike.Models;

namespace Spike.Application.Services.Abstractions;

public interface IChatService
{
    Task<Message> BeginConversationAsync(Patient patient, CancellationToken cancellationToken);
    Task<Message> SummarizeConversationAsync(List<Message> messages, CancellationToken cancellationToken);
    Task<Message> GetReply(List<Message> messages, CancellationToken cancellationToken);
}