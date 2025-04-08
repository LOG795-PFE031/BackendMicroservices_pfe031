using Portfolio.Proxies.Dtos;

namespace Portfolio.Proxies;

public interface ITimeProxy
{
    Task<CurrentTime?> GetCurrentTime();
}