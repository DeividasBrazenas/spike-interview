using ElevenLabs.TextToSpeech;
using ElevenLabs.Voices;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using Serilog;
using Spike.Application.Clients;

namespace Spike.Infrastructure.Speech;

internal class ElevenLabsClient : ITextToSpeechClient
{
    private readonly ElevenLabs.ElevenLabsClient _client;

    private readonly AsyncRetryPolicy _retryPolicy;

    private readonly IMemoryCache _voicesCache;
    private readonly TimeSpan _voiceCacheExpiration = TimeSpan.FromMinutes(5);

    public ElevenLabsClient(ElevenLabs.ElevenLabsClient client, ILogger logger)
    {
        _client = client;

        _retryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(retryCount: 3, onRetry: (exception, _, retryCount) => { logger.Warning(exception, "Retrying Deepgram request. Attempt #{RetryCount}", retryCount); });

        _voicesCache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<byte[]> SynthesizeAsync(string text, string voiceId, CancellationToken cancellationToken)
    {
        var voice = await _voicesCache.GetOrCreateAsync<Voice>(voiceId, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _voiceCacheExpiration;

            var voiceResponse = await _retryPolicy.ExecuteAsync(ct => _client.VoicesEndpoint.GetVoiceAsync(voiceId, true, ct), cancellationToken);

            return voiceResponse;
        });

        var request = new TextToSpeechRequest(voice, text);

        var response = await _retryPolicy.ExecuteAsync(ct => _client.TextToSpeechEndpoint.TextToSpeechAsync(request, null, ct), cancellationToken);

        return response.ClipData.ToArray();
    }
}
