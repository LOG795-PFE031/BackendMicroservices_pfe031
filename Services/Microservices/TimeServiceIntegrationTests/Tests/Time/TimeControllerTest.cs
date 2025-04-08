using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Time.Commands.TimeMultiplier;
using Time.Controllers;
using TimeServiceIntegrationTests.Infrastructure;

namespace TimeServiceIntegrationTests.Tests.Time;

[Collection(nameof(TestCollections.Default))]
public sealed class TimeControllerTest
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public TimeControllerTest(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidAuth_SetTimeMultiplier_ShouldReturnOk()
    {
        const int multiplier = 2;
        
        var testId = Guid.NewGuid();
        
        var client = _applicationFactoryFixture.WithAdminAuth();

        var response = await client.PatchAsJsonAsync("time", new ChangeClockTimeMultiplier(multiplier));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithValidAuth_GetTime_ShouldReturnCurrentUtcTime()
    {
        var testId = Guid.NewGuid();

        var client = _applicationFactoryFixture.WithAdminAuth();

        var response = await client.GetFromJsonAsync<TimeController.CurrentTime>("time");

        response.Should().NotBeNull();

        response.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromDays(5));
    }
}