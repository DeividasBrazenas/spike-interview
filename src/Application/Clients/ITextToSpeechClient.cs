namespace Spike.Application.Clients;

public interface ITextToSpeechClient
{
    Task<byte[]> SynthesizeAsync(string text, string voiceId, CancellationToken cancellationToken);
}