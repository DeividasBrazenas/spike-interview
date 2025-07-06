using Moq.AutoMock;
using Spike.Hub.Mappers;
using Spike.Tests.Common.Fixture;

namespace Spike.Tests.Common.Base;

public abstract class TestBase
{
    protected AutoFixture.Fixture Fixture = null!;
    protected AutoMocker Mocker = null!;

    protected TestBase()
    {
        Fixture = new AutoFixture.Fixture();

        Fixture.Customize(new DateOnlyCustomization());
    }

    [SetUp]
    public void BaseSetUp()
    {
        Mocker = new AutoMocker();
        Mocker.With<IMapper, Mapper>();
    }
}