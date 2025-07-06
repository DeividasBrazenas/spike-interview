using Deepgram.Clients.Interfaces.v1;
using Polly;
using Polly.Retry;
using Serilog;
using Spike.Application.Clients;

namespace Spike.Infrastructure.Speech;

internal class DeepgramClient : ISpeechToTextClient
{
    private readonly IListenRESTClient _client;
    private readonly AsyncRetryPolicy _retryPolicy;
    
    public DeepgramClient(IListenRESTClient client, ILogger logger)
    {
        _client = client;
        _retryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(retryCount: 3, onRetry: (exception, _, retryCount) => { logger.Warning(exception, "Retrying Deepgram request. Attempt #{RetryCount}", retryCount); });
    }

    public async Task<string?> TranscribeAsync(byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        var response = await _retryPolicy.ExecuteAsync(ct => _client.TranscribeFile(audioBytes, null, CancellationTokenSource.CreateLinkedTokenSource(ct)), cancellationToken);
        
        return response.Results?.Channels?.FirstOrDefault()?.Alternatives?.FirstOrDefault()?.Transcript;
    }
}