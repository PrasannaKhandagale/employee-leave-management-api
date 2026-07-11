using LeaveManagement.Api.Models;

namespace LeaveManagement.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var manager = new User
        {
            Id = 201, Name = "Rahul Manager", Email = "manager@test.com",
            Password = "Password@123", Role = UserRole.Manager,
            Department = "Engineering", JoiningDate = new DateOnly(2021, 1, 10)
        };
        var employee = new User
        {
            Id = 101, Name = "Anita Sharma", Email = "employee@test.com",
            Password = "Password@123", Role = UserRole.Employee,
            Department = "Engineering", JoiningDate = new DateOnly(2024, 7, 1), ManagerId = 201
        };
        var admin = new User
        {
            Id = 301, Name = "System Admin", Email = "admin@test.com",
            Password = "Password@123", Role = UserRole.Admin,
            Department = "IT", JoiningDate = new DateOnly(2020, 1, 1)
        };

        db.Users.AddRange(employee, manager, admin);
        db.LeaveBalances.AddRange(
            new LeaveBalance { EmployeeId = 101, LeaveType = LeaveType.Casual, Total = 12, Used = 3 },
            new LeaveBalance { EmployeeId = 101, LeaveType = LeaveType.Sick, Total = 10, Used = 2 },
            new LeaveBalance { EmployeeId = 101, LeaveType = LeaveType.Earned, Total = 15, Used = 1 });
        db.SaveChanges();
    }
}
