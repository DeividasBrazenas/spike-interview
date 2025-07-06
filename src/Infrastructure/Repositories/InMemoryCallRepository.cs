using System.Collections.Concurrent;
using Spike.Application.Repositories;
using Spike.Models;

namespace Spike.Infrastructure.Repositories;

internal class InMemoryCallRepository : ICallRepository
{
    private readonly ConcurrentDictionary<Guid, Call> _calls;

    public InMemoryCallRepository()
    {
        _calls = new ConcurrentDictionary<Guid, Call>();
    }
    
    public Task AddAsync(Call call, CancellationToken cancellationToken)
    {
        if (!_calls.TryAdd(call.Id, call))
        {
            throw new Exception("Failed to add call to repository");
        }
        
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Call call, CancellationToken cancellationToken)
    {
        _calls.AddOrUpdate(call.Id, call, (_, _) => call);
        
        return Task.CompletedTask;
    }

    public Task<Call?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_calls.GetValueOrDefault(id));
    }
}