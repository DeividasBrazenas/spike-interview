using MessagePack;

namespace Spike.Hub.Contracts.Requests;

[MessagePackObject(keyAsPropertyName: true)]
public record StartCallRequest(
    [property: Key("PatientDto")] PatientDto Patient,
    [property: Key("VoiceId")] string VoiceId
);