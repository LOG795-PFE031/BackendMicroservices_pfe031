using Stock.Commands.Seedwork;

namespace Stock.Commands.Quotes;

public sealed record AddQuote(
    string Symbol,
    DateTime Date,
    decimal Price,
    string ModelType,
    decimal Confidence,
    string ModelVersion
) : ICommand;