using MessagePack;

namespace Spike.Hub.Contracts;

[MessagePackObject(keyAsPropertyName: true)]
public record CallSummaryDto(
    [property: Key("ReferenceNumber")] string? ReferenceNumber,
    [property: Key("VisitLimit")] string? VisitLimit,
    [property: Key("VisitLimitStructure")] string? VisitLimitStructure,
    [property: Key("VisitsUsed")] string? VisitsUsed,
    [property: Key("Copay")] string? Copay,
    [property: Key("Deductible")] string? Deductible,
    [property: Key("DeductibleMet")] string? DeductibleMet,
    [property: Key("OutOfPocketMaximum")] string? OutOfPocketMaximum,
    [property: Key("OutOfPocketMet")] string? OutOfPocketMet,
    [property: Key("InitialAuthorizationRequired")] string? InitialAuthorizationRequired
);