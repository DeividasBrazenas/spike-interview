using Spike.Hub.Contracts;
using Spike.Models;

namespace Spike.Hub.Mappers;

public class Mapper : IMapper
{
    public Patient MapPatient(PatientDto patient)
    {
        return new Patient(patient.FirstName, patient.LastName, DateOnly.Parse(patient.DateOfBirth),
            patient.MemberId, DateTimeOffset.Parse(patient.InsuranceActiveAsOf), DateTimeOffset.Parse(patient.DateOfTreatment));
    }

    public CallSummaryDto MapCallSummary(CallSummary? summary)
    {
        return new CallSummaryDto(summary?.ReferenceNumber, summary?.VisitLimit, summary?.VisitLimitStructure,
            summary?.VisitsUsed, summary?.Copay, summary?.Deductible,
            summary?.DeductibleMet, summary?.OutOfPocketMaximum,
            summary?.OutOfPocketMet, summary?.InitialAuthorizationRequired);
    }
}