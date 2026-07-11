using LeaveManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<LeaveBalance>().HasIndex(x => new { x.EmployeeId, x.LeaveType }).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.Role).HasConversion<string>();
        modelBuilder.Entity<LeaveBalance>().Property(x => x.LeaveType).HasConversion<string>();
        modelBuilder.Entity<LeaveRequest>().Property(x => x.LeaveType).HasConversion<string>();
        modelBuilder.Entity<LeaveRequest>().Property(x => x.Status).HasConversion<string>();
    }
}
