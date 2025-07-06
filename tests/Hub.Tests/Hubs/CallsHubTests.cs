using System.Globalization;
using AutoFixture;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Spike.Application.Exceptions;
using Spike.Application.Services.Abstractions;
using Spike.Hub.Contracts;
using Spike.Hub.Contracts.Requests;
using Spike.Hub.Hubs;
using Spike.Hub.Validators;
using Spike.Models;
using Spike.Tests.Common.Base;

namespace Spike.Hub.Tests.Hubs;

[TestFixture]
public class CallsHubTests : TestBase
{
    private CallsHub _hub;

    private Mock<IDtoValidator> ValidatorMock => Mocker.GetMock<IDtoValidator>();
    private Mock<ICallService> CallServiceMock => Mocker.GetMock<ICallService>();
    private Mock<IHubCallerClients> HubCallerClientsMock => Mocker.GetMock<IHubCallerClients>();
    private Mock<ISingleClientProxy> SingleClientProxyMock => Mocker.GetMock<ISingleClientProxy>();
    private Mock<HubCallerContext> HubCallerContextMock => Mocker.GetMock<HubCallerContext>();

    [SetUp]
    public void SetUp()
    {
        HubCallerClientsMock
            .Setup(m => m.Caller)
            .Returns(SingleClientProxyMock.Object);

        HubCallerContextMock
            .Setup(m => m.ConnectionAborted)
            .Returns(CancellationToken.None);

        _hub = Mocker.CreateInstance<CallsHub>();
        _hub.Context = HubCallerContextMock.Object;
        _hub.Clients = HubCallerClientsMock.Object;
    }

    [TearDown]
    public void TearDown()
    {
        _hub.Dispose();
    }

    [Test]
    [AutoData]
    public void StartCall_InvalidPatient_ThrowsException(StartCallRequest request, string exceptionMessage)
    {
        // Arrange
        ValidatorMock
            .Setup(m => m.Validate(It.IsAny<PatientDto>()))
            .Throws(() => new ValidationException(exceptionMessage));

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _hub.StartCall(request), exceptionMessage);
    }

    [Test]
    [AutoData]
    public async Task StartCall_ReturnsCallId()
    {
        // Arrange
        var call = Fixture.Create<Call>();
        var patientDto = Fixture.Build<PatientDto>()
            .With(p => p.DateOfBirth, DateTime.Now.ToString("yyyy-MM-dd"))
            .With(p => p.InsuranceActiveAsOf, DateTime.Now.ToString(CultureInfo.InvariantCulture))
            .With(p => p.DateOfTreatment, DateTime.Now.ToString(CultureInfo.InvariantCulture))
            .Create();

        CallServiceMock
            .Setup(m => m.StartCall(It.IsAny<Patient>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(call);

        // Act
        var result = await _hub.StartCall(new StartCallRequest(patientDto, "voice"));

        // Assert
        Assert.That(result, Is.EqualTo(call.Id.ToString()));
    }

    [Test]
    [AutoData]
    public void EndCall_InvalidCallId_ThrowsException(EndCallRequest request)
    {
        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _hub.EndCall(request), "CallId is not a valid GUID.");
    }

    [Test]
    [AutoData]
    public async Task EndCall_SendsSummary(Guid callId)
    {
        // Arrange
        var call = Fixture.Create<Call>();

        CallServiceMock
            .Setup(m => m.EndCall(callId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(call);

        // Act
        await _hub.EndCall(new EndCallRequest(callId.ToString()));

        // Assert
        SingleClientProxyMock
            .Verify(m => m.SendCoreAsync("ReceiveCallSummary", It.Is<object?[]>(a => AreEqual((CallSummaryDto)a[0]!, call.Summary!)), It.IsAny<CancellationToken>()));
    }

    [Test]
    [AutoData]
    public void Interact_InvalidCallId_ThrowsException(string callId, byte[] audioBytes)
    {
        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _hub.Interact(callId, audioBytes), "CallId is not a valid GUID.");
    }

    [Test]
    [AutoData]
    public async Task Interact_SendsAudio(Guid callId, byte[] audioBytes, byte[] result)
    {
        // Arrange
        CallServiceMock
            .Setup(m => m.Interact(callId, audioBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _hub.Interact(callId.ToString(), audioBytes);

        // Assert
        SingleClientProxyMock
            .Verify(m => m.SendCoreAsync("ReceiveAudio", It.Is<object?[]>(a => AreEqual((byte[])a[0]!, result)), It.IsAny<CancellationToken>()));
    }

    private bool AreEqual(CallSummaryDto expected, CallSummary actual)
    {
        return expected.ReferenceNumber == actual.ReferenceNumber &&
               expected.VisitLimit == actual.VisitLimit &&
               expected.VisitLimitStructure == actual.VisitLimitStructure &&
               expected.VisitsUsed == actual.VisitsUsed &&
               expected.Copay == actual.Copay &&
               expected.Deductible == actual.Deductible &&
               expected.DeductibleMet == actual.DeductibleMet &&
               expected.OutOfPocketMaximum == actual.OutOfPocketMaximum &&
               expected.OutOfPocketMet == actual.OutOfPocketMet &&
               expected.InitialAuthorizationRequired == actual.InitialAuthorizationRequired;
    }

    private bool AreEqual(byte[] expected, byte[] actual)
    {
        return expected.SequenceEqual(actual);
    }
}