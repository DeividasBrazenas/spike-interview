using AutoFixture;
using AutoFixture.NUnit3;
using Moq;
using Spike.Application.Clients;
using Spike.Application.Repositories;
using Spike.Application.Services;
using Spike.Models;
using Spike.Tests.Common.Base;

namespace Spike.Application.Tests.Services;

[TestFixture]
public class ChatServiceTests : TestBase
{
    private ChatService _service;

    private Mock<IPromptsRepository> PromptsRepositoryMock => Mocker.GetMock<IPromptsRepository>();
    private Mock<IChatClient> ChatClientMock => Mocker.GetMock<IChatClient>();

    [SetUp]
    public void SetUp()
    {
        _service = Mocker.CreateInstance<ChatService>();
    }

    [Test]
    [AutoData]
    public async Task BeginConversationAsync_ReturnsMessageWithPrompt(string prompt)
    {
        // Arrange
        var patient = Fixture.Create<Patient>();

        PromptsRepositoryMock
            .Setup(m => m.GetPromptAsync("BeginConversationPrompt", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prompt);

        // Act
        var result = await _service.BeginConversationAsync(patient, CancellationToken.None);

        // Assert
        Assert.That(result.Role, Is.EqualTo(MessageRole.System));
        Assert.That(result.Content, Is.EqualTo(prompt));
    }

    [Test]
    [AutoData]
    public async Task SummarizeConversationAsync_ReturnsReply(string prompt, List<Message> messages, Message reply)
    {
        // Arrange
        PromptsRepositoryMock
            .Setup(m => m.GetPromptAsync("SummarizeConversationPrompt", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prompt);

        ChatClientMock
            .Setup(m => m.GetReply(It.IsAny<List<Message>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reply);

        // Act
        var result = await _service.SummarizeConversationAsync(messages, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(reply));

        PromptsRepositoryMock.Verify(m => m.GetPromptAsync("SummarizeConversationPrompt", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
        ChatClientMock.Verify(m => m.GetReply(It.IsAny<List<Message>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [AutoData]
    public async Task GetReply_ReturnsMessageFromClient(List<Message> messages, Message reply)
    {
        // Arrange
        ChatClientMock
            .Setup(m => m.GetReply(It.IsAny<List<Message>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reply);

        // Act
        var result = await _service.GetReply(messages, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(reply));

        ChatClientMock.Verify(m => m.GetReply(It.IsAny<List<Message>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}