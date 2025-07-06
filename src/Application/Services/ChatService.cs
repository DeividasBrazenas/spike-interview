using Spike.Application.Clients;
using Spike.Application.Repositories;
using Spike.Application.Services.Abstractions;
using Spike.Models;

namespace Spike.Application.Services;

internal class ChatService : IChatService
{
    private readonly IChatClient _chatClient;
    private readonly IPromptsRepository _promptsRepository;

    private const string BeginConversationPromptId = "BeginConversationPrompt";
    private const string SummarizeConversationPromptId = "SummarizeConversationPrompt";

    public ChatService(IChatClient chatClient, IPromptsRepository promptsRepository)
    {
        _chatClient = chatClient;
        _promptsRepository = promptsRepository;
    }

    /// <summary>
    /// Returns a message to begin a conversation with the bot.
    /// </summary>
    /// <param name="patient">Patient details to be supplied to LLM</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Message> BeginConversationAsync(Patient patient, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            { "FirstName", patient.FirstName },
            { "LastName", patient.LastName },
            { "DateOfBirth", patient.DateOfBirth.ToString("O") },
            { "MemberId", patient.MemberId },
            { "InsuranceActiveAsOf", patient.InsuranceActiveAsOf.ToString("G") },
            { "DateOfTreatment", patient.DateOfTreatment.ToString("G") }
        };

        var prompt = await _promptsRepository.GetPromptAsync(BeginConversationPromptId, parameters, cancellationToken);

        var message = new Message(MessageRole.System, prompt);

        return message;
    }

    /// <summary>
    /// Summarizes the conversation and returns a message with the summary.
    /// </summary>
    /// <param name="messages">Conversation messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Message with the summary json object</returns>
    public async Task<Message> SummarizeConversationAsync(List<Message> messages, CancellationToken cancellationToken)
    {
        var prompt = await _promptsRepository.GetPromptAsync(SummarizeConversationPromptId, new Dictionary<string, string>(), cancellationToken);

        var message = new Message(MessageRole.System, prompt);
        messages.Add(message);

        var reply = await GetReply(messages, cancellationToken);

        return reply;
    }

    /// <summary>
    /// Generates a reply for the conversation based on the provided messages.
    /// </summary>
    /// <param name="messages">Conversation messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated message</returns>
    public async Task<Message> GetReply(List<Message> messages, CancellationToken cancellationToken)
    {
        var reply = await _chatClient.GetReply(messages, cancellationToken);

        return reply;
    }
}