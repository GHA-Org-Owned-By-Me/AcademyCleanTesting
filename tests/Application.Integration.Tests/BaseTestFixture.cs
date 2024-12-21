namespace Application.IntegrationTests;
using static Testing;

public abstract class BaseTestFixture
{
    [SetUp]
    public async Task TestSetup()
    {
        await ResetState();
    }
}
