using ComprasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth-test")]
public class AuthTestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        var userInfo = new
        {
            IsAuthenticated = User.Identity.IsAuthenticated,
            UserName = User.Identity.Name,
            UserId = User.FindFirst("sub")?.Value,
            ClientId = User.FindFirst("client_id")?.Value,
            AllClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
        };

        return Ok(userInfo);
    }
}