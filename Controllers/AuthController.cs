using LeaveManagement.Api.Data;
using LeaveManagement.Api.Models;
using LeaveManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower());
        if (user is null || user.Password != request.Password)
            return Unauthorized(new { errorCode = "INVALID_CREDENTIALS", message = "Email or password is incorrect." });

        return Ok(new
        {
            accessToken = tokens.Create(user), expiresIn = 3600,
            user = new { user.Id, user.Name, user.Email, role = user.Role.ToString() }
        });
    }
}
