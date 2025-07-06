using Spike.Hub.Contracts;
using Spike.Models;

namespace Spike.Hub.Mappers;

public interface IMapper
{
    Patient MapPatient(PatientDto patient);
    CallSummaryDto MapCallSummary(CallSummary? summary);
}