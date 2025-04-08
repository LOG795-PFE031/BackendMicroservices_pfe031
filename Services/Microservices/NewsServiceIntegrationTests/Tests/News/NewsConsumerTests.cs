using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using News.Commands.Interfaces;
using News.Interfaces;
using NewsServiceIntegrationTests.Infrastructure;
using NewsServiceIntegrationTests.Tests.News.Services;

namespace NewsServiceIntegrationTests.Tests.News;

[Collection(nameof(TestCollections.Default))]
public sealed class NewsConsumerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public NewsConsumerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidNews_NewsConsumer_ShouldIndexNewsSuccessfully()
    {
        const string symbol = "AAPL";

        DateTime date = DateTime.UtcNow;

        const string content = "Today we saw...";

        await NewsPublisher.PublishAsync(_applicationFactoryFixture, content);

        await Task.Delay(1_000);

        using var scope = _applicationFactoryFixture.Services.CreateScope();

        IArticleRepository newsRepository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
        IAzureBlobRepository azureBlobRepository = scope.ServiceProvider.GetRequiredService<IAzureBlobRepository>();

        var article = await newsRepository.GetBySymbolAsync(symbol);

        article.Should().NotBeNull();

        article.PublishedAt.Date.Should().Be(date.Date);

        var blob = await azureBlobRepository.DownloadBlobAsync(article.ContentId);

        blob.Should().Be(content);
    }
}