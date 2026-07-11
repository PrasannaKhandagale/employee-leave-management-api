using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Api.Models;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record CreateEmployeeRequest(
    [Required, MinLength(2), MaxLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string Department,
    DateOnly JoiningDate,
    int? ManagerId,
    UserRole Role = UserRole.Employee);

public record CreateLeaveRequest(
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    [Required, MinLength(3), MaxLength(500)] string Reason);

public record UpdateLeaveStatusRequest(
    LeaveStatus Status,
    [MaxLength(500)] string? Comments);
