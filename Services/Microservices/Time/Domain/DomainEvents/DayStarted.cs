using Time.Domain.Seedwork.Abstract;

namespace Time.Domain.DomainEvents;

public sealed class DayStarted : Event
{
    public DateTime NewDay { get; }

    public DayStarted(DateTime newDay)
    {
        NewDay = newDay;
    }
}