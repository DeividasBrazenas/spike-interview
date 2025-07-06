using OpenAI.Chat;
using Polly;
using Polly.Retry;
using Serilog;
using Spike.Application.Clients;
using Spike.Application.Exceptions;
using Spike.Models;

namespace Spike.Infrastructure.LLM;

public class OpenAiClient : IChatClient
{
    private readonly ChatClient _client;
    private readonly AsyncRetryPolicy _retryPolicy;

    public OpenAiClient(ChatClient client, ILogger logger)
    {
        _client = client;
        _retryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(retryCount: 3, onRetry: (exception, _, retryCount) => { logger.Warning(exception, "Retrying OpenAI request. Attempt #{RetryCount}", retryCount); });
    }

    public async Task<Message> GetReply(List<Message> messages, CancellationToken cancellationToken)
    {
        var openAiMessages = messages.Select(ToOpenAiMessage);

        var response = await _retryPolicy.ExecuteAsync(ct => _client.CompleteChatAsync(openAiMessages, null, ct), cancellationToken);

        var replyMessage = response.Value.Content.FirstOrDefault();
        if (replyMessage is null)
        {
            throw new DomainException("No reply message received from OpenAI.");
        }

        var reply = ToModelMessage(replyMessage.Text);

        return reply;
    }

    private static ChatMessage ToOpenAiMessage(Message message) =>
        message.Role switch
        {
            MessageRole.User => new UserChatMessage(message.Content),
            MessageRole.Assistant => new AssistantChatMessage(message.Content),
            MessageRole.System => new SystemChatMessage(message.Content),
            _ => throw new NotImplementedException($"Unknown role {message.Role}")
        };

    private static Message ToModelMessage(string content)
    {
        return new Message(MessageRole.Assistant, content);
    }
}