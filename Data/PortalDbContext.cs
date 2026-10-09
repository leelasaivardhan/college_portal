using Microsoft.EntityFrameworkCore;
using MultiCollegePortal.Models;

namespace MultiCollegePortal.Data;

public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options) { }
    public DbSet<College> Colleges => Set<College>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Notice> Notices => Set<Notice>();
    public DbSet<Period> Periods => Set<Period>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Mark> Marks => Set<Mark>();
    public DbSet<AcademicCalendarEntry> AcademicCalendarEntries => Set<AcademicCalendarEntry>();
    public DbSet<LibraryLoan> LibraryLoans => Set<LibraryLoan>();
    public DbSet<FeePayment> FeePayments => Set<FeePayment>();
    public DbSet<DownloadRecord> DownloadRecords => Set<DownloadRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<College>().HasMany(x => x.Users).WithOne(x => x.College).HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<College>().HasMany(x => x.Notices).WithOne(x => x.College).HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<College>().HasMany(x => x.Periods).WithOne(x => x.College).HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Period>().HasOne(x => x.Faculty).WithMany().HasForeignKey(x => x.FacultyId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Mark>().HasIndex(x => new { x.StudentId, x.Subject, x.Assessment }).IsUnique();
        modelBuilder.Entity<College>().HasMany(x => x.CalendarEntries).WithOne(x => x.College).HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<College>().HasMany(x => x.LibraryLoans).WithOne(x => x.College).HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<College>().HasMany(x => x.FeePayments).WithOne(x => x.College).HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<College>().HasMany(x => x.DownloadRecords).WithOne(x => x.College).HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Cascade);
    }
}
