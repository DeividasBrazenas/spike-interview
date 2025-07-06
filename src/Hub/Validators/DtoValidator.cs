using System.Text.RegularExpressions;
using Spike.Application.Exceptions;
using Spike.Hub.Contracts;

namespace Spike.Hub.Validators;

public class DtoValidator : IDtoValidator
{
    private static readonly Regex BirthDateRegex = new Regex(@"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$");
    private static readonly Regex DateTimeRegex = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})$");

    public void Validate(PatientDto patient)
    {
        if (string.IsNullOrEmpty(patient.FirstName))
        {
            throw new ValidationException($"{nameof(patient.FirstName)} is required.");
        }

        if (string.IsNullOrEmpty(patient.LastName))
        {
            throw new ValidationException($"{nameof(patient.LastName)} is required.");
        }

        if (string.IsNullOrEmpty(patient.DateOfBirth))
        {
            throw new ValidationException($"{nameof(patient.DateOfBirth)} is required.");
        }

        if (!BirthDateRegex.IsMatch(patient.DateOfBirth))
        {
            throw new ValidationException($"{nameof(patient.DateOfBirth)} is not valid or has incorrect format (expected: YYYY-MM-DD).");
        }

        if (string.IsNullOrEmpty(patient.MemberId))
        {
            throw new ValidationException($"{nameof(patient.MemberId)} is required.");
        }

        if (string.IsNullOrEmpty(patient.InsuranceActiveAsOf))
        {
            throw new ValidationException($"{nameof(patient.InsuranceActiveAsOf)} is required.");
        }

        if (!DateTimeRegex.IsMatch(patient.InsuranceActiveAsOf))
        {
            throw new ValidationException($"{nameof(patient.InsuranceActiveAsOf)} is not a valid ISO string.");
        }

        if (string.IsNullOrEmpty(patient.DateOfTreatment))
        {
            throw new ValidationException($"{nameof(patient.DateOfTreatment)} is required.");
        }

        if (!DateTimeRegex.IsMatch(patient.DateOfTreatment))
        {
            throw new ValidationException($"{nameof(patient.DateOfTreatment)} is not a valid ISO string.");
        }
    }
}