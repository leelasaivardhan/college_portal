using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiCollegePortal.Models;

namespace MultiCollegePortal.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(PortalDbContext db)
    {
        if (await db.Colleges.AnyAsync()) return;

        var colleges = new[]
        {
            new College { Name = "Government Polytechnic Visakhapatnam", ShortName = "GPV", Address = "Visakhapatnam, Andhra Pradesh" },
            new College { Name = "ABC Engineering College", ShortName = "ABC", Address = "Andhra Pradesh" },
            new College { Name = "XYZ Polytechnic College", ShortName = "XYZ", Address = "Andhra Pradesh" },
            new College { Name = "DEF Institute of Technology", ShortName = "DEF", Address = "Andhra Pradesh" },
            new College { Name = "GHI College", ShortName = "GHI", Address = "Andhra Pradesh" }
        };
        db.Colleges.AddRange(colleges);
        await db.SaveChangesAsync();

        var hasher = new PasswordHasher<AppUser>();
        var admin = new AppUser { FullName = "Portal Administrator", Email = "admin@portal.local", Role = UserRole.Admin, CollegeId = colleges[0].Id, IsActive = true };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
        var student = new AppUser
        {
            FullName = "Demo Student", Email = "student@portal.local", Role = UserRole.Student, CollegeId = colleges[0].Id,
            RollNumber = "GPV2026001", Department = "Computer Science", Course = "B.Tech", Year = 4, Semester = 7,
            IsActive = true, IsHosteller = true, HostelRoom = "A-204"
        };
        student.PasswordHash = hasher.HashPassword(student, "Student@123");
        var faculty = new AppUser
        {
            FullName = "Demo Faculty", Email = "faculty@portal.local", Role = UserRole.Faculty, CollegeId = colleges[0].Id,
            Department = "Computer Science", IsActive = true
        };
        faculty.PasswordHash = hasher.HashPassword(faculty, "Faculty@123");
        db.Users.AddRange(admin, student, faculty);
        await db.SaveChangesAsync();

        var today = DateTime.Today;
        db.Notices.AddRange(
            new Notice { CollegeId = colleges[0].Id, Title = "Scholarship Applications Open", Content = "Scholarship applications are open. Students should submit the required documents before the announced deadline.", Category = "Scholarship", PublishedOn = DateTime.UtcNow, IsPublished = true },
            new Notice { CollegeId = colleges[0].Id, Title = "B.Tech II Year Examination Schedule", Content = "The examination schedule is available in the Examination section.", Category = "Examination", PublishedOn = DateTime.UtcNow.AddDays(-1), IsPublished = true },
            new Notice { CollegeId = colleges[0].Id, Title = "Campus Recruitment - TCS", Content = "Eligible final-year students can register for the upcoming campus recruitment drive.", Category = "Placement", PublishedOn = DateTime.UtcNow.AddDays(-2), IsPublished = true }
        );

        db.Periods.AddRange(
            new Period { CollegeId = colleges[0].Id, WeekDate = today, Day = today.DayOfWeek, TimeSlot = "09:00 - 10:00", Subject = "Data Structures", ClassName = "CSE IV Year", Room = "Room 101", FacultyId = faculty.Id },
            new Period { CollegeId = colleges[0].Id, WeekDate = today, Day = today.DayOfWeek, TimeSlot = "10:00 - 11:00", Subject = "Web Technologies", ClassName = "CSE IV Year", Room = "Room 102", FacultyId = faculty.Id },
            new Period { CollegeId = colleges[0].Id, WeekDate = today, Day = today.DayOfWeek, TimeSlot = "11:30 - 12:30", Subject = "Database Management", ClassName = "CSE IV Year", Room = "Room 101", FacultyId = faculty.Id }
        );

        db.Marks.AddRange(
            new Mark { CollegeId = colleges[0].Id, FacultyId = faculty.Id, StudentId = student.Id, Subject = "Data Structures", Assessment = "Mid1", Score = 25, MaxScore = 30 },
            new Mark { CollegeId = colleges[0].Id, FacultyId = faculty.Id, StudentId = student.Id, Subject = "Web Technologies", Assessment = "Lab1", Score = 27, MaxScore = 30 }
        );
        db.Attendances.AddRange(
            new Attendance { CollegeId = colleges[0].Id, FacultyId = faculty.Id, StudentId = student.Id, AttendanceDate = today.AddDays(-1), Subject = "Data Structures", Present = true },
            new Attendance { CollegeId = colleges[0].Id, FacultyId = faculty.Id, StudentId = student.Id, AttendanceDate = today.AddDays(-2), Subject = "Web Technologies", Present = true },
            new Attendance { CollegeId = colleges[0].Id, FacultyId = faculty.Id, StudentId = student.Id, AttendanceDate = today.AddDays(-3), Subject = "Database Management", Present = false }
        );
        db.LibraryLoans.Add(new LibraryLoan { CollegeId = colleges[0].Id, StudentId = student.Id, BookTitle = "Clean Code", BookCode = "LIB-1008", IssuedOn = today.AddDays(-8) });
        db.FeePayments.AddRange(
            new FeePayment { CollegeId = colleges[0].Id, StudentId = student.Id, FeeType = "Tuition Fee", Amount = 35000, IsPaid = false, DueDate = today.AddDays(12) },
            new FeePayment { CollegeId = colleges[0].Id, StudentId = student.Id, FeeType = "Library Fee", Amount = 1000, IsPaid = true, DueDate = today.AddDays(-10) }
        );
        db.AcademicCalendarEntries.AddRange(
            new AcademicCalendarEntry { CollegeId = colleges[0].Id, SemesterName = "Semester 1", StartDate = new DateTime(today.Year, 1, 1), EndDate = new DateTime(today.Year, 6, 30), Description = "First six months academic cycle" },
            new AcademicCalendarEntry { CollegeId = colleges[0].Id, SemesterName = "Semester 2", StartDate = new DateTime(today.Year, 7, 1), EndDate = new DateTime(today.Year, 12, 31), Description = "Second six months academic cycle" }
        );
        db.DownloadRecords.Add(new DownloadRecord { CollegeId = colleges[0].Id, StudentId = student.Id, FileName = "Academic Calendar 2026-27.pdf", DownloadedOn = DateTime.UtcNow.AddDays(-1) });
        await db.SaveChangesAsync();
    }
}
