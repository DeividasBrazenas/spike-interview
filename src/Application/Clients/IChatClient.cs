using Spike.Models;

namespace Spike.Application.Clients;

public interface IChatClient
{
    Task<Message> GetReply(List<Message> messages, CancellationToken cancellationToken);
}