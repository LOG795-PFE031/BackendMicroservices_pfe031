using Stock.Commands.Seedwork;

namespace Stock.Commands.Quotes;

public sealed record AddQuote(string Symbol, DateTime Day, decimal Decimal) : ICommand;