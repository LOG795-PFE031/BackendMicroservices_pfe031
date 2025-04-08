using News.Domain.Seedwork.Abstract;

namespace News.Consumers.Messages;

public sealed class News : Event
{
    public string Title { get; init; }
    public string Symbol { get; init; }
    public string Content { get; init; }
    public DateTime PublishedAt { get; init; }
    public int Opinion { get; init; }
}