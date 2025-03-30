using AuthNuget.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TestWebApplication;

[Authorize(Roles = RoleConstants.AdminRole)]
[ApiController]
[Route("secure")]
public sealed class SecuredController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello, Admin!");
    }
}