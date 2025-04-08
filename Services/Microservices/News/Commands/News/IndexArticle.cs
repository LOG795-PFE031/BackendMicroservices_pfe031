using News.Commands.Seedwork;

namespace News.Commands.News;

public sealed record IndexArticle(string Title, string SymbolId, string Content, DateTime PublishedAt, int Opinion) : ICommand;