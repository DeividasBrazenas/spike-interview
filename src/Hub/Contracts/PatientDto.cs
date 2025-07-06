using MessagePack;

namespace Spike.Hub.Contracts;

[MessagePackObject(keyAsPropertyName: true)]
public record PatientDto(
    [property: Key("FirstName")] string FirstName,
    [property: Key("LastName")] string LastName,
    [property: Key("DateOfBirth")] string DateOfBirth,
    [property: Key("MemberId")] string MemberId,
    [property: Key("InsuranceActiveAsOf")] string InsuranceActiveAsOf,
    [property: Key("DateOfTreatment")] string DateOfTreatment
);