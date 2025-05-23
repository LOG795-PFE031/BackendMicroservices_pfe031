﻿using RabbitMqNuget.Testing;
using Stock.Consumers.Messages;
using StockServiceIntegrationTests.Infrastructure;

namespace StockServiceIntegrationTests.Tests.Stocks.Services;

public sealed class QuotePublisher
{
    public static async Task<StockQuote> PublishQuote(ApplicationFactoryFixture applicationFactoryFixture, string symbol)
    {
        const decimal price = 100;

        DateTime date = DateTime.UtcNow;

        var quote = new StockQuote
        {
            Symbol = symbol,
            Price = price,
            Date = date,
        };

        return await applicationFactoryFixture.Services.WithMessagePublished(quote);
    }
}