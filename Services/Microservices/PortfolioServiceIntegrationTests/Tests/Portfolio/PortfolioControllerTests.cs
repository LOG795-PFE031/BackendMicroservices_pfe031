using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Commands.Interfaces;
using Portfolio.Domain;
using Portfolio.Domain.ValueObjects;
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

        using var scope = _applicationFactoryFixture.Services.CreateScope();

        var walletRepository = scope.ServiceProvider.GetRequiredService<IWalletRepository>();

        await walletRepository.AddAsync(new Wallet("I'm a fake wallet Id", new Money(100_000), new List<ShareVolume>()));

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