using Spike.Hub.Contracts;

namespace Spike.Hub.Validators;

public interface IDtoValidator
{
    void Validate(PatientDto patient);
}