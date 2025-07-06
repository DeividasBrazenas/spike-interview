using AutoFixture;
using Spike.Application.Exceptions;
using Spike.Hub.Contracts;
using Spike.Hub.Validators;
using Spike.Tests.Common.Base;

namespace Spike.Hub.Tests.Validators;

[TestFixture]
public class DtoValidatorTests : TestBase
{
    private DtoValidator _dtoValidator;

    [SetUp]
    public void SetUp()
    {
        _dtoValidator = Mocker.CreateInstance<DtoValidator>();
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void ValidatePatient_FirstNameIsEmpty_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.FirstName, value).Create();

        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "FirstName is required.");
    }
    
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void ValidatePatient_LastNameIsEmpty_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.LastName, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "LastName is required.");
    }
    
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void ValidatePatient_DateOfBirthIsEmpty_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.DateOfBirth, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "DateOfBirth is required.");
    }
    
    [TestCase("abc")]
    [TestCase("1-2-3")]
    [TestCase("2023-13-01")]
    public void ValidatePatient_DateOfBirthNotValid_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.DateOfBirth, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "DateOfBirth is not valid or has incorrect format (expected: YYYY-MM-DD).");
    }
    
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void ValidatePatient_MemberIdIsEmpty_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.MemberId, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "MemberId is required.");
    }
    
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void ValidatePatient_InsuranceActiveAsOfIsEmpty_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.InsuranceActiveAsOf, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "InsuranceActiveAsOf is required.");
    }
    
    [TestCase("abc")]
    [TestCase("1-2-3")]
    [TestCase("2023-13-01")]
    public void ValidatePatient_InsuranceActiveAsOfNotValid_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.InsuranceActiveAsOf, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "InsuranceActiveAsOf is not a valid ISO string.");
    }
    
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void ValidatePatient_DateOfTreatmentIsEmpty_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.DateOfTreatment, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "DateOfTreatment is required.");
    }
    
    [TestCase("abc")]
    [TestCase("1-2-3")]
    [TestCase("2023-13-01")]
    public void ValidatePatient_DateOfTreatmentNotValid_ThrowsException(string? value)
    {
        // Arrange
        var patient = Fixture.Build<PatientDto>().With(p => p.DateOfTreatment, value).Create();
    
        // Act & Assert
        Assert.Throws<ValidationException>(() => _dtoValidator.Validate(patient), "DateOfTreatment is not a valid ISO string.");
    }
    
    [Test]
    public void ValidatePatient_ValidPayload_DoesNotThrowException()
    {
        // Arrange
        var patient = new PatientDto("FirstName", "LastName", "2025-07-05", "MemberId", "2025-07-05T00:00:00Z", "2025-07-05T00:00:00Z");
    
        // Act & Assert
        Assert.DoesNotThrow(() => _dtoValidator.Validate(patient));
    }
}