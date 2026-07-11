using System.Security.Claims;
using LeaveManagement.Api.Data;
using LeaveManagement.Api.Models;
using LeaveManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Api.Controllers;

[ApiController]
[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestsController(AppDbContext db, LeaveCalculator calculator) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpPost]
    [Authorize(Roles="Employee")]
    public async Task<IActionResult> Create(CreateLeaveRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.StartDate < today) return BadRequest(new { errorCode="PAST_START_DATE", message="Start date cannot be in the past." });
        if (request.EndDate < request.StartDate) return BadRequest(new { errorCode="INVALID_DATE_RANGE", message="End date cannot be earlier than start date." });
        var days = calculator.WorkingDays(request.StartDate, request.EndDate);
        if (days == 0) return BadRequest(new { errorCode="NO_WORKING_DAYS", message="The selected range contains no working days." });

        var overlap = await db.LeaveRequests.AnyAsync(x => x.EmployeeId == CurrentUserId &&
            x.Status != LeaveStatus.Rejected && x.Status != LeaveStatus.Cancelled &&
            request.StartDate <= x.EndDate && request.EndDate >= x.StartDate);
        if (overlap) return Conflict(new { errorCode="OVERLAPPING_LEAVE", message="A leave request already exists for overlapping dates." });

        var balance = await db.LeaveBalances.SingleAsync(x => x.EmployeeId==CurrentUserId && x.LeaveType==request.LeaveType);
        if (days > balance.Available) return Conflict(new { errorCode="INSUFFICIENT_BALANCE", message="Available leave balance is insufficient.", available=balance.Available, requested=days });

        var leave = new LeaveRequest { EmployeeId=CurrentUserId, LeaveType=request.LeaveType, StartDate=request.StartDate,
            EndDate=request.EndDate, NumberOfDays=days, Reason=request.Reason };
        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { leaveRequestId=leave.Id }, ToResponse(leave));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] LeaveStatus? status, [FromQuery] int page=1, [FromQuery] int pageSize=10)
    {
        if (page < 1 || pageSize is < 1 or > 100) return BadRequest(new { errorCode="INVALID_PAGINATION", message="Page must be at least 1 and pageSize must be between 1 and 100." });
        IQueryable<LeaveRequest> query = db.LeaveRequests;
        if (CurrentRole == "Employee") query = query.Where(x=>x.EmployeeId==CurrentUserId);
        else if (CurrentRole == "Manager")
        {
            var employeeIds = db.Users.Where(x=>x.ManagerId==CurrentUserId).Select(x=>x.Id);
            query = query.Where(x=>employeeIds.Contains(x.EmployeeId));
        }
        if (status is not null) query=query.Where(x=>x.Status==status);
        var total=await query.CountAsync();
        var items=await query.OrderByDescending(x=>x.CreatedAtUtc).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
        return Ok(new { page, pageSize, total, totalPages=(int)Math.Ceiling(total/(double)pageSize), items=items.Select(ToResponse) });
    }

    [HttpGet("{leaveRequestId:int}")]
    public async Task<IActionResult> GetById(int leaveRequestId)
    {
        var leave=await db.LeaveRequests.FindAsync(leaveRequestId);
        if (leave is null) return NotFound(new { errorCode="LEAVE_NOT_FOUND", message="Leave request was not found." });
        if (!await CanAccess(leave)) return Forbid();
        return Ok(ToResponse(leave));
    }

    [HttpPatch("{leaveRequestId:int}/status")]
    [Authorize(Roles="Manager")]
    public async Task<IActionResult> UpdateStatus(int leaveRequestId, UpdateLeaveStatusRequest request)
    {
        if (request.Status is not (LeaveStatus.Approved or LeaveStatus.Rejected))
            return BadRequest(new { errorCode="INVALID_STATUS", message="Manager can only approve or reject a request." });
        var leave=await db.LeaveRequests.FindAsync(leaveRequestId);
        if (leave is null) return NotFound(new { errorCode="LEAVE_NOT_FOUND", message="Leave request was not found." });
        var employee=await db.Users.FindAsync(leave.EmployeeId);
        if (employee?.ManagerId != CurrentUserId) return Forbid();
        if (leave.Status != LeaveStatus.Pending) return Conflict(new { errorCode="STATUS_ALREADY_FINAL", message="Only pending requests can be approved or rejected." });

        if (request.Status==LeaveStatus.Approved)
        {
            var balance=await db.LeaveBalances.SingleAsync(x=>x.EmployeeId==leave.EmployeeId && x.LeaveType==leave.LeaveType);
            if (leave.NumberOfDays > balance.Available) return Conflict(new { errorCode="INSUFFICIENT_BALANCE", message="Balance is no longer sufficient." });
            balance.Used += leave.NumberOfDays;
        }
        leave.Status=request.Status; leave.ManagerComments=request.Comments; leave.UpdatedAtUtc=DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ToResponse(leave));
    }

    [HttpPost("{leaveRequestId:int}/cancel")]
    [Authorize(Roles="Employee")]
    public async Task<IActionResult> Cancel(int leaveRequestId)
    {
        var leave=await db.LeaveRequests.FindAsync(leaveRequestId);
        if (leave is null) return NotFound(new { errorCode="LEAVE_NOT_FOUND", message="Leave request was not found." });
        if (leave.EmployeeId != CurrentUserId) return Forbid();
        if (leave.Status is LeaveStatus.Rejected or LeaveStatus.Cancelled) return Conflict(new { errorCode="CANNOT_CANCEL", message="This request cannot be cancelled." });
        if (leave.Status==LeaveStatus.Approved)
        {
            if (leave.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow)) return Conflict(new { errorCode="LEAVE_ALREADY_STARTED", message="Approved leave cannot be cancelled on or after its start date." });
            var balance=await db.LeaveBalances.SingleAsync(x=>x.EmployeeId==leave.EmployeeId && x.LeaveType==leave.LeaveType);
            balance.Used -= leave.NumberOfDays;
        }
        leave.Status=LeaveStatus.Cancelled; leave.UpdatedAtUtc=DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ToResponse(leave));
    }

    private async Task<bool> CanAccess(LeaveRequest leave)
    {
        if (CurrentRole=="Admin") return true;
        if (CurrentRole=="Employee") return leave.EmployeeId==CurrentUserId;
        var employee=await db.Users.FindAsync(leave.EmployeeId);
        return employee?.ManagerId==CurrentUserId;
    }

    private static object ToResponse(LeaveRequest x) => new { leaveRequestId=x.Id, x.EmployeeId,
        leaveType=x.LeaveType.ToString().ToUpperInvariant(), x.StartDate, x.EndDate, x.NumberOfDays,
        x.Reason, status=x.Status.ToString().ToUpperInvariant(), x.ManagerComments, x.CreatedAtUtc, x.UpdatedAtUtc };
}
