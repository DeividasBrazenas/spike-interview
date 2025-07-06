using MessagePack;

namespace Spike.Hub.Contracts.Requests;

[MessagePackObject(keyAsPropertyName: true)]
public record EndCallRequest(
    [property: Key("CallId")] string CallId
);