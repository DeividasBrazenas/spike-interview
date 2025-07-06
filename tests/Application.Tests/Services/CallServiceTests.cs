using System.Text.Json;
using AutoFixture;
using AutoFixture.NUnit3;
using Moq;
using Spike.Application.Clients;
using Spike.Application.Exceptions;
using Spike.Application.Repositories;
using Spike.Application.Services;
using Spike.Application.Services.Abstractions;
using Spike.Models;
using Spike.Tests.Common.Base;

namespace Spike.Application.Tests.Services;

[TestFixture]
public class CallServiceTests : TestBase
{
    private CallService _service;

    private Mock<IChatService> ChatServiceMock => Mocker.GetMock<IChatService>();
    private Mock<ICallRepository> CallRepositoryMock => Mocker.GetMock<ICallRepository>();
    private Mock<ISpeechToTextClient> SpeechToTextClientMock => Mocker.GetMock<ISpeechToTextClient>();
    private Mock<ITextToSpeechClient> TextToSpeechClientMock => Mocker.GetMock<ITextToSpeechClient>();

    [SetUp]
    public void SetUp()
    {
        _service = Mocker.CreateInstance<CallService>();
    }

    [Test]
    [AutoData]
    public async Task StartCall_WorksAsExpected(Message message, string voiceId)
    {
        // Arrange
        var patient = Fixture.Create<Patient>();

        ChatServiceMock
            .Setup(m => m.BeginConversationAsync(patient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        // Act
        var result = await _service.StartCall(patient, voiceId, CancellationToken.None);

        // Assert
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Patient, Is.EqualTo(patient));
        Assert.That(result.Status, Is.EqualTo(CallStatus.Started));
        Assert.That(result.Messages.Single(), Is.EqualTo(message));

        ChatServiceMock.Verify(m => m.BeginConversationAsync(patient, It.IsAny<CancellationToken>()), Times.Once);
        CallRepositoryMock.Verify(m => m.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [AutoData]
    public void EndCall_CallNotFound_ThrowsException(Guid callId, Message message)
    {
        // Arrange
        CallRepositoryMock
            .Setup(m => m.GetAsync(callId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Call?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _service.EndCall(callId, CancellationToken.None));

        CallRepositoryMock.Verify(m => m.GetAsync(callId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [AutoData]
    public async Task EndCall_WorksAsExpected(CallSummary summary)
    {
        // Arrange
        var call = Fixture.Create<Call>();

        CallRepositoryMock
            .Setup(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(call);

        var summaryMessage = new Message(MessageRole.System, JsonSerializer.Serialize(summary));

        ChatServiceMock
            .Setup(m => m.SummarizeConversationAsync(call.Messages, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaryMessage);

        // Act
        var result = await _service.EndCall(call.Id, CancellationToken.None);

        // Assert
        Assert.That(result.Id, Is.EqualTo(call.Id));
        Assert.That(result.Messages, Does.Contain(summaryMessage));
        Assert.That(result.Summary, Is.EqualTo(summary));

        CallRepositoryMock.Verify(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()), Times.Once);
        ChatServiceMock.Verify(m => m.SummarizeConversationAsync(It.IsAny<List<Message>>(), It.IsAny<CancellationToken>()), Times.Once);
        CallRepositoryMock.Verify(m => m.UpdateAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [AutoData]
    public async Task Interact_EmptyAudioBytes_ReturnsEmptyResult(Guid callId)
    {
        // Act
        var result = await _service.Interact(callId, [], CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);

        CallRepositoryMock.Verify(m => m.GetAsync(callId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    [AutoData]
    public void Interact_CallNotFound_ThrowsException(Guid callId, byte[] audioBytes)
    {
        // Arrange
        CallRepositoryMock
            .Setup(m => m.GetAsync(callId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Call?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _service.Interact(callId, audioBytes, CancellationToken.None), $"Call with Id '{callId}' was not found.");

        CallRepositoryMock.Verify(m => m.GetAsync(callId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [AutoData]
    public void Interact_CallAlreadyEnded_ThrowsException(byte[] audioBytes)
    {
        // Arrange
        var call = Fixture.Build<Call>()
            .With(c => c.Status, CallStatus.Ended)
            .Create();

        CallRepositoryMock
            .Setup(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(call);

        // Act & Assert
        Assert.ThrowsAsync<DomainException>(async () => await _service.Interact(call.Id, audioBytes, CancellationToken.None), $"Call with Id '{call.Id}' has already ended");

        CallRepositoryMock.Verify(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [InlineAutoData(CallStatus.Started)]
    [InlineAutoData(CallStatus.Active)]
    public async Task Interact_EmptyTranscribedAudio_ReturnsEmptyResult(CallStatus callStatus, byte[] audioBytes)
    {
        // Arrange
        var call = Fixture.Build<Call>()
            .With(c => c.Status, callStatus)
            .Create();

        CallRepositoryMock
            .Setup(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(call);

        SpeechToTextClientMock
            .Setup(m => m.TranscribeAsync(audioBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _service.Interact(call.Id, audioBytes, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);

        CallRepositoryMock.Verify(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [AutoData]
    public async Task Interact_WorksAsExpected(byte[] audioBytes, string transcribedAudio, Message assistantResponse, byte[] resultAudioBytes)
    {
        // Arrange
        var call = Fixture.Build<Call>()
            .With(c => c.Status, CallStatus.Active)
            .Create();

        CallRepositoryMock
            .Setup(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(call);

        SpeechToTextClientMock
            .Setup(m => m.TranscribeAsync(audioBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcribedAudio);

        ChatServiceMock
            .Setup(m => m.GetReply(call.Messages, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assistantResponse);

        TextToSpeechClientMock
            .Setup(m => m.SynthesizeAsync(assistantResponse.Content, call.VoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultAudioBytes);

        // Act
        var result = await _service.Interact(call.Id, audioBytes, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(resultAudioBytes));

        CallRepositoryMock.Verify(m => m.GetAsync(call.Id, It.IsAny<CancellationToken>()), Times.Once);
        SpeechToTextClientMock.Verify(m => m.TranscribeAsync(audioBytes, It.IsAny<CancellationToken>()), Times.Once);
        ChatServiceMock.Verify(m => m.GetReply(It.Is<List<Message>>(p => p.Contains(new Message(MessageRole.User, transcribedAudio))), It.IsAny<CancellationToken>()), Times.Once);
        TextToSpeechClientMock.Verify(m => m.SynthesizeAsync(assistantResponse.Content, call.VoiceId, It.IsAny<CancellationToken>()), Times.Once);
        CallRepositoryMock.Verify(m => m.UpdateAsync(It.Is<Call>(c => c.Id == call.Id && c.Status == CallStatus.Active && c.Messages.Contains(assistantResponse)), It.IsAny<CancellationToken>()), Times.Once);
    }
}