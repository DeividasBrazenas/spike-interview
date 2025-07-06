namespace Spike.Models;

public record Patient(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string MemberId,
    DateTimeOffset InsuranceActiveAsOf,
    DateTimeOffset DateOfTreatment
);
