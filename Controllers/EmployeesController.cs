using System.Security.Claims;
using LeaveManagement.Api.Data;
using LeaveManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateEmployeeRequest request)
    {
        if (await db.Users.AnyAsync(x => x.Email.ToLower() == request.Email.ToLower()))
            return Conflict(new { errorCode = "EMAIL_EXISTS", message = "An employee with this email already exists." });
        if (request.ManagerId is not null && !await db.Users.AnyAsync(x => x.Id == request.ManagerId && x.Role == UserRole.Manager))
            return BadRequest(new { errorCode = "INVALID_MANAGER", message = "Manager ID is invalid." });

        var user = new User { Name=request.Name, Email=request.Email, Password=request.Password, Department=request.Department,
            JoiningDate=request.JoiningDate, ManagerId=request.ManagerId, Role=request.Role };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        if (user.Role == UserRole.Employee)
        {
            db.LeaveBalances.AddRange(Enum.GetValues<LeaveType>().Select(t => new LeaveBalance { EmployeeId=user.Id, LeaveType=t, Total=t==LeaveType.Earned?15:10 }));
            await db.SaveChangesAsync();
        }
        return CreatedAtAction(nameof(GetById), new { employeeId=user.Id }, new { user.Id, user.Name, user.Email, role=user.Role.ToString(), user.Department, user.JoiningDate, user.ManagerId });
    }

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetById(int employeeId)
    {
        var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Employee" && currentId != employeeId) return Forbid();
        var user = await db.Users.FindAsync(employeeId);
        return user is null ? NotFound(new { errorCode="EMPLOYEE_NOT_FOUND", message="Employee was not found." })
            : Ok(new { user.Id, user.Name, user.Email, role=user.Role.ToString(), user.Department, user.JoiningDate, user.ManagerId });
    }

    [HttpGet("{employeeId:int}/leave-balance")]
    public async Task<IActionResult> GetBalance(int employeeId)
    {
        var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Employee" && currentId != employeeId) return Forbid();
        if (!await db.Users.AnyAsync(x => x.Id == employeeId)) return NotFound(new { errorCode="EMPLOYEE_NOT_FOUND", message="Employee was not found." });
        var balances = await db.LeaveBalances.Where(x=>x.EmployeeId==employeeId).ToListAsync();
        return Ok(new { employeeId, balances = balances.Select(x=>new { leaveType=x.LeaveType.ToString().ToUpperInvariant(), x.Total, x.Used, x.Available }) });
    }
}
