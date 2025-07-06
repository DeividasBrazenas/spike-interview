using Spike.Infrastructure.LLM;
using Spike.Tests.Common.Base;

namespace Spike.Infrastructure.Tests.LLM;

[TestFixture]
public class OpenAiClientTests : TestBase
{
    private OpenAiClient _openAiClient;
    
    // TODO: Injected OpenAI.Chat.ChatClient cannot be mocked directly due to its internal structure.
}