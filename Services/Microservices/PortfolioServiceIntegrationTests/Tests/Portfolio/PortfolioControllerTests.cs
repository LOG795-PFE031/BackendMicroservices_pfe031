using System.Net.Http.Json;
using Bogus;
using FakeItEasy;
using FluentAssertions;
using Portfolio.Proxies;
using Portfolio.Proxies.Dtos;
using Portfolio.Queries.ShareVolume;
using PortfolioServiceIntegrationTests.Infrastructure;

namespace PortfolioServiceIntegrationTests.Tests.Portfolio;

[Collection(nameof(TestCollections.Default))]
public sealed class PortfolioControllerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public PortfolioControllerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidConfiguration_GetShareVolumes_ShouldReturnValidShareVolumesViewModel()
    {
        var fake = new Faker();
        var symbol = fake.Company.CompanyName();

        var stockProxy = A.Fake<IStockProxy>();
        var timeProxy = A.Fake<ITimeProxy>();
        var authProxy = A.Fake<IAuthProxy>();

        A.CallTo(() => stockProxy.GetStockPrice(A<string>._, A<DateTime>._)).Returns(new StockPrice(1_000));
        A.CallTo(() => timeProxy.GetCurrentTime()).Returns(new CurrentTime(DateTime.UtcNow));
        A.CallTo(() => authProxy.GetWalletIdAsync()).Returns(new WalletId("I'm a fake wallet Id"));

        var client = _applicationFactoryFixture.WithAdminAuth();

        var response = await client.PatchAsync($"portfolio/buy/{symbol}/{10}", null);

        response.EnsureSuccessStatusCode();

        var shareVolumes = await client.GetFromJsonAsync<ShareVolumesViewModel>("portfolio");

        shareVolumes.Should().NotBeNull();

        shareVolumes.ShareVolumes.Should().NotBeEmpty();

        shareVolumes.ShareVolumes.Should().ContainSingle(x => x.Symbol == symbol);

        shareVolumes.ShareVolumes.First(x => x.Symbol == symbol).Volume.Should().Be(10);
    }
}