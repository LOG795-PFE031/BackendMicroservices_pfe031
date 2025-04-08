using Stock.Commands.Seedwork;

namespace Stock.Commands.Share;

public sealed record CreateShare(string Symbol) : ICommand;