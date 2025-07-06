namespace Spike.Application.Clients;

public interface ISpeechToTextClient
{
    Task<string?> TranscribeAsync(byte[] audioBytes, CancellationToken cancellationToken);
}