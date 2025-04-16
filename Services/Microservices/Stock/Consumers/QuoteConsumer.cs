using MassTransit;
using Stock.Commands.Quotes;
using Stock.Commands.Seedwork;
using Stock.Consumers.Messages;

namespace Stock.Consumers;

public sealed class QuoteConsumer : IConsumer<StockQuote>
{
    private readonly ICommandDispatcher _commandDispatcher;

    public QuoteConsumer(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    public async Task Consume(ConsumeContext<StockQuote> context)
    {
        var quote = context.Message;
        await _commandDispatcher.DispatchAsync(new AddQuote(
            quote.Symbol, 
            quote.Date, 
            quote.Price,
            quote.ModelType,
            quote.Confidence,
            quote.ModelVersion
        ));
    }
}