using Spike.Models;

namespace Spike.Application.Services.Abstractions;

public interface ICallService
{
    Task<Call> StartCall(Patient patient, string voiceId, CancellationToken cancellationToken);
    Task<Call> EndCall(Guid callId, CancellationToken cancellationToken);
    
    Task<byte[]> Interact(Guid callId, byte[] audioBytes, CancellationToken cancellationToken);
}