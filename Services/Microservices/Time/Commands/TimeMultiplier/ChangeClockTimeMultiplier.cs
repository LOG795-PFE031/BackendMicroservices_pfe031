using Time.Commands.Seedwork;

namespace Time.Commands.TimeMultiplier;

public sealed record ChangeClockTimeMultiplier(int Multiplier) : ICommand;