namespace Spike.Models;

public record Call(Guid Id, Patient Patient, CallStatus Status, List<Message> Messages, string VoiceId)
{
    public CallSummary? Summary { get; init; }
};