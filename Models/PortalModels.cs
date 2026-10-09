using System.ComponentModel.DataAnnotations;

namespace MultiCollegePortal.Models;

public enum UserRole { Student, Faculty, Admin }

public class College
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(100)] public string ShortName { get; set; } = "";
    [MaxLength(500)] public string? LogoUrl { get; set; }
    [MaxLength(1000)] public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Notice> Notices { get; set; } = new List<Notice>();
    public ICollection<Period> Periods { get; set; } = new List<Period>();
    public ICollection<AcademicCalendarEntry> CalendarEntries { get; set; } = new List<AcademicCalendarEntry>();
    public ICollection<LibraryLoan> LibraryLoans { get; set; } = new List<LibraryLoan>();
    public ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();
    public ICollection<DownloadRecord> DownloadRecords { get; set; } = new List<DownloadRecord>();
}

public class AppUser
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string FullName { get; set; } = "";
    [Required, MaxLength(320), EmailAddress] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; }
    public int? CollegeId { get; set; }
    public College? College { get; set; }
    [MaxLength(50)] public string? RollNumber { get; set; }
    [MaxLength(100)] public string? Department { get; set; }
    [MaxLength(50)] public string? Course { get; set; }
    public int Year { get; set; } = 1;
    public int Semester { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool IsHosteller { get; set; }
    [MaxLength(50)] public string? HostelRoom { get; set; }
}

public class Notice
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public College? College { get; set; }
    [Required, MaxLength(250)] public string Title { get; set; } = "";
    [Required] public string Content { get; set; } = "";
    [MaxLength(50)] public string Category { get; set; } = "General";
    public DateTime PublishedOn { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = true;
}

public class Period
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public College? College { get; set; }
    public DateTime WeekDate { get; set; }
    public DayOfWeek Day { get; set; }
    [Required, MaxLength(50)] public string TimeSlot { get; set; } = "";
    [Required, MaxLength(150)] public string Subject { get; set; } = "";
    [MaxLength(150)] public string ClassName { get; set; } = "";
    [MaxLength(100)] public string Room { get; set; } = "";
    public int? FacultyId { get; set; }
    public AppUser? Faculty { get; set; }
}

public class Attendance
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public int FacultyId { get; set; }
    public int StudentId { get; set; }
    public DateTime AttendanceDate { get; set; }
    [Required, MaxLength(150)] public string Subject { get; set; } = "";
    public bool Present { get; set; }
}

public class Mark
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public int FacultyId { get; set; }
    public int StudentId { get; set; }
    [Required, MaxLength(150)] public string Subject { get; set; } = "";
    [Required, MaxLength(100)] public string Assessment { get; set; } = "";
    [Range(0, 30)] public decimal Score { get; set; }
    public decimal MaxScore { get; set; } = 30;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
}

public class AcademicCalendarEntry
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public College? College { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [Required, MaxLength(100)] public string SemesterName { get; set; } = "";
    [MaxLength(500)] public string Description { get; set; } = "";
}

public class LibraryLoan
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public College? College { get; set; }
    public int StudentId { get; set; }
    [Required, MaxLength(250)] public string BookTitle { get; set; } = "";
    [MaxLength(100)] public string BookCode { get; set; } = "";
    public DateTime IssuedOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
}

public class FeePayment
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public College? College { get; set; }
    public int StudentId { get; set; }
    [MaxLength(150)] public string FeeType { get; set; } = "Tuition Fee";
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime DueDate { get; set; }
}

public class DownloadRecord
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public College? College { get; set; }
    public int StudentId { get; set; }
    [Required, MaxLength(250)] public string FileName { get; set; } = "";
    public DateTime DownloadedOn { get; set; } = DateTime.UtcNow;
}

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required] public UserRole Role { get; set; } = UserRole.Student;
    public string? ReturnUrl { get; set; }
}

public class MarkEntryViewModel
{
    [Required] public int StudentId { get; set; }
    [Required] public string Subject { get; set; } = "";
    [Required] public string Assessment { get; set; } = "";
    [Range(0, 30)] public decimal Score { get; set; }
}

public class AttendanceEntryViewModel
{
    [Required] public int StudentId { get; set; }
    [Required] public string Subject { get; set; } = "";
    public DateTime AttendanceDate { get; set; } = DateTime.Today;
    public bool Present { get; set; } = true;
}

public class AddUserViewModel
{
    [Required, MaxLength(200)] public string FullName { get; set; } = "";
    [Required, EmailAddress, MaxLength(320)] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(6)] public string Password { get; set; } = "";
    [Required] public UserRole Role { get; set; }
    public int CollegeId { get; set; }
    public string? RollNumber { get; set; }
    public string? Department { get; set; }
    public string? Course { get; set; }
    public int Year { get; set; } = 1;
    public int Semester { get; set; } = 1;
}
