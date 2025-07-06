using System.Text.Json;
using Serilog;
using Spike.Application.Clients;
using Spike.Application.Exceptions;
using Spike.Application.Repositories;
using Spike.Application.Services.Abstractions;
using Spike.Models;

namespace Spike.Application.Services;

internal class CallService : ICallService
{
    private readonly ISpeechToTextClient _speechToTextClient;
    private readonly ITextToSpeechClient _textToSpeechClient;
    private readonly IChatService _chatService;
    private readonly ICallRepository _callRepository;
    private readonly ILogger _logger;

    public CallService(ISpeechToTextClient speechToTextClient, ITextToSpeechClient textToSpeechClient, IChatService chatService, ICallRepository callRepository, ILogger logger)
    {
        _speechToTextClient = speechToTextClient;
        _textToSpeechClient = textToSpeechClient;
        _chatService = chatService;
        _callRepository = callRepository;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a new call between the insurer and bot.
    /// </summary>
    /// <param name="patient">Patient details</param>
    /// <param name="voiceId">Assistant's voice Id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Call> StartCall(Patient patient, string voiceId, CancellationToken cancellationToken)
    {
        var message = await _chatService.BeginConversationAsync(patient, cancellationToken);

        var call = new Call(Guid.NewGuid(), patient, CallStatus.Started, [message], voiceId);

        await _callRepository.AddAsync(call, cancellationToken);

        return call;
    }

    /// <summary>
    /// Ends a call between the insurer and bot.
    /// </summary>
    /// <param name="callId">Call identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Call> EndCall(Guid callId, CancellationToken cancellationToken)
    {
        var call = await _callRepository.GetAsync(callId, cancellationToken);
        if (call is null)
        {
            throw new NotFoundException($"Call with Id '{callId} was not found.");
        }

        var messages = call.Messages;

        var summaryMessage = await _chatService.SummarizeConversationAsync(call.Messages, cancellationToken);
        messages.Add(summaryMessage);

        CallSummary? summary = null;
        try
        {
            summary = JsonSerializer.Deserialize<CallSummary>(summaryMessage.Content);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to deserialize call summary for call {CallId}. Content: {Content}", callId, summaryMessage.Content);
        }

        call = call with { Status = CallStatus.Ended, Summary = summary };

        await _callRepository.UpdateAsync(call, cancellationToken);

        return call;
    }

    /// <summary>
    /// Interacts with insurer during the ongoing call
    /// </summary>
    /// <param name="callId">Call Identifier</param>
    /// <param name="audioBytes">Audio bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response audio bytes</returns>
    public async Task<byte[]> Interact(Guid callId, byte[] audioBytes, CancellationToken cancellationToken)
    {
        if (audioBytes.Length == 0)
        {
            return [];
        }
        
        var call = await _callRepository.GetAsync(callId, cancellationToken);
        if (call is null)
        {
            throw new NotFoundException($"Call with Id '{callId}' was not found.");
        }

        if (call.Status is CallStatus.Ended)
        {
            throw new DomainException($"Call with Id '{callId}' has already ended");
        }

        var transcribedAudio = await _speechToTextClient.TranscribeAsync(audioBytes, cancellationToken);
        if (transcribedAudio == "")
        {
            return [];
        }

        call = call with { Status = CallStatus.Active };

        var messages = call.Messages;

        var userMessage = new Message(MessageRole.User, transcribedAudio!);
        messages.Add(userMessage);

        var reply = await _chatService.GetReply(messages, cancellationToken);
        messages.Add(reply);

        await _callRepository.UpdateAsync(call, cancellationToken);

        var synthesizedAudio = await _textToSpeechClient.SynthesizeAsync(reply.Content, call.VoiceId, cancellationToken);

        return synthesizedAudio;
    }
}