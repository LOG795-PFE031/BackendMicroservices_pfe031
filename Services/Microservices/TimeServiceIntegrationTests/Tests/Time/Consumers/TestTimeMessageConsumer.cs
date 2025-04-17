using MassTransit;
using Time.Domain.DomainEvents;
using TimeServiceIntegrationTests.Infrastructure;

namespace TimeServiceIntegrationTests.Tests.Time.Consumers;

public sealed class TestTimeMessageConsumer : IConsumer<DayStarted>
{
    public Task Consume(ConsumeContext<DayStarted> context)
    {
        MessageSink.AddMessage(context.Message);

        return Task.CompletedTask;
    }
}