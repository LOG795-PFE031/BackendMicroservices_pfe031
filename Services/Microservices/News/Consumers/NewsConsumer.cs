using MassTransit;
using News.Commands.News;
using News.Commands.Seedwork;

namespace News.Consumers;

public class NewsConsumer : IConsumer<Messages.News>
{
    private readonly ICommandDispatcher _commandDispatcher;

    public NewsConsumer(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    public async Task Consume(ConsumeContext<Messages.News> context)
    {
        Messages.News news = context.Message;

        IndexArticle command = new IndexArticle(news.Title, news.Symbol, news.Content, news.PublishedAt, news.Opinion);

        var result = await _commandDispatcher.DispatchAsync(command);

        result.ThrowIfException();
    }
}