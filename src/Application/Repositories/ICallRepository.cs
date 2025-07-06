using Spike.Models;

namespace Spike.Application.Repositories;

public interface ICallRepository
{
    Task AddAsync(Call call, CancellationToken cancellationToken);
    
    Task UpdateAsync(Call call, CancellationToken cancellationToken);

    Task<Call?> GetAsync(Guid id, CancellationToken cancellationToken);
}