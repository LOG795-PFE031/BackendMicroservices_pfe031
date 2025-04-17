using Microsoft.AspNetCore.Http;

namespace AuthNuget.Http;

public sealed class AuthDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor? _contextAccessor;

    public AuthDelegatingHandler(IHttpContextAccessor? contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_contextAccessor?.HttpContext is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        if (_contextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        request.Headers.Add("Authorization", $"Bearer {authorizationHeader}");
        
        return await base.SendAsync(request, cancellationToken);
    }
}