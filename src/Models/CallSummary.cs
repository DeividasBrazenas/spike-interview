namespace Spike.Models;

public record CallSummary(
    string ReferenceNumber,
    string VisitLimit,
    string VisitLimitStructure,
    string VisitsUsed,
    string Copay,
    string Deductible,
    string DeductibleMet,
    string OutOfPocketMaximum,
    string OutOfPocketMet,
    string InitialAuthorizationRequired);