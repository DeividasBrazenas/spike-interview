using AutoFixture.NUnit3;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Models.Listen.v1.REST;
using Moq;
using Spike.Infrastructure.Speech;
using Spike.Tests.Common.Base;

namespace Spike.Infrastructure.Tests.Speech;

[TestFixture]
public class DeepgramClientTests : TestBase
{
    private DeepgramClient _client;

    private Mock<IListenRESTClient> ListenClientMock => Mocker.GetMock<IListenRESTClient>();

    [SetUp]
    public void SetUp()
    {
        _client = Mocker.CreateInstance<DeepgramClient>();
    }

    [Test]
    [AutoData]
    public async Task TranscribeAsync_WorksAsExpected(byte[] audioBytes, SyncResponse response)
    {
        // Arrange
        ListenClientMock
            .Setup(m => m.TranscribeFile(audioBytes, It.IsAny<PreRecordedSchema>(), It.IsAny<CancellationTokenSource>(), null, null))
            .ReturnsAsync(response);
        
        // Act
        var result = await _client.TranscribeAsync(audioBytes);
        
        // Assert
        Assert.That(result, Is.EqualTo(response.Results?.Channels?.FirstOrDefault()?.Alternatives?.FirstOrDefault()?.Transcript));
        
        ListenClientMock.Verify(m => m.TranscribeFile(audioBytes, It.IsAny<PreRecordedSchema>(), It.IsAny<CancellationTokenSource>(), null, null), Times.Once);
    }
}