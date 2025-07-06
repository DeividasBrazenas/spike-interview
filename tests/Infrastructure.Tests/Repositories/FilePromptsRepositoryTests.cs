using AutoFixture.NUnit3;
using Spike.Infrastructure.Repositories;
using Spike.Tests.Common.Base;

namespace Spike.Infrastructure.Tests.Repositories;

[TestFixture]
public class FilePromptsRepositoryTests : TestBase
{
    private FilePromptsRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = Mocker.CreateInstance<FilePromptsRepository>();
    }

    [Test]
    [AutoData]
    public void GetPromptAsync_FileDoesNotExist_ThrowsException(string promptId)
    {
        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(() => _repository.GetPromptAsync(promptId, null, CancellationToken.None));
    }

    [Test]
    public async Task GetPromptAsync_PromptWithoutParameters_ReturnsPrompt()
    {
        // Act
        var result = await _repository.GetPromptAsync("PromptWithoutParameters", null, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo("Prompt without parameters"));
    }

    [Test]
    public async Task GetPromptAsync_PromptWithParameters_ReturnsPrompt()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "Parameter1", "A" },
            { "Parameter2", "B" },
            { "Parameter3", "C" },
        };
        
        // Act
        var result = await _repository.GetPromptAsync("PromptWithParameters", parameters, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo("Prompt with parameters: A, B, C"));
    }
    
    [Test]
    public async Task GetPromptAsync_PromptWithParametersWithoutParametersSupplied_ReturnsPrompt()
    {
        // Act
        var result = await _repository.GetPromptAsync("PromptWithParameters", null, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo("Prompt with parameters: {Parameter1}, {Parameter2}, {Parameter3}"));
    }
}