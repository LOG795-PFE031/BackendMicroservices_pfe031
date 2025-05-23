﻿using System.Net.Http.Json;
using FluentAssertions;
using News.Queries.News;
using NewsServiceIntegrationTests.Infrastructure;
using NewsServiceIntegrationTests.Tests.News.Services;

namespace NewsServiceIntegrationTests.Tests.News;

[Collection(nameof(TestCollections.Default))]
public sealed class NewsController
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public NewsController(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidAuth_GetNews_ShouldDownloadContent()
    {
        const string lorem = "Lorem ipsum dolor sit amet.";

        await NewsPublisher.PublishAsync(_applicationFactoryFixture, lorem);

        var client = _applicationFactoryFixture.WithAdminAuth();

        var news = await client.GetFromJsonAsync<NewsViewModel>($"news/{NewsPublisher.Symbol}");

        news.Should().NotBeNull();

        var response = await client.GetAsync($"news/articles/{news!.ArticleViewModels.Last().Id}", HttpCompletionOption.ResponseContentRead);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be(lorem);
    }
}