namespace LeaveManagement.Api.Models;

public enum UserRole { Employee, Manager, Admin }
public enum LeaveType { Casual, Sick, Earned }
public enum LeaveStatus { Pending, Approved, Rejected, Cancelled }

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public UserRole Role { get; set; }
    public string Department { get; set; } = "General";
    public DateOnly JoiningDate { get; set; }
    public int? ManagerId { get; set; }
}

public class LeaveBalance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public int Total { get; set; }
    public int Used { get; set; }
    public int Available => Total - Used;
}

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int NumberOfDays { get; set; }
    public required string Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ManagerComments { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
