using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiCollegePortal.Data;
using MultiCollegePortal.Models;

namespace MultiCollegePortal.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly PortalDbContext _db;
    public StudentController(PortalDbContext db) => _db = db;
    private int StudentId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int CollegeId => int.Parse(User.FindFirstValue("CollegeId")!);

    private async Task LoadCommon()
    {
        ViewBag.College = await _db.Colleges.FindAsync(CollegeId);
        ViewBag.Student = await _db.Users.FindAsync(StudentId);
    }

    public async Task<IActionResult> Dashboard()
    {
        await LoadCommon();
        ViewBag.Notices = await _db.Notices.Where(x => x.CollegeId == CollegeId && x.IsPublished).OrderByDescending(x => x.PublishedOn).Take(6).ToListAsync();
        ViewBag.TodayPeriods = await _db.Periods.Where(x => x.CollegeId == CollegeId && x.Day == DateTime.Today.DayOfWeek).OrderBy(x => x.TimeSlot).Take(6).ToListAsync();
        ViewBag.AttendanceCount = await _db.Attendances.CountAsync(x => x.StudentId == StudentId);
        ViewBag.PendingFees = await _db.FeePayments.Where(x => x.StudentId == StudentId && !x.IsPaid).SumAsync(x => (decimal?)x.Amount) ?? 0;
        return View();
    }

    public async Task<IActionResult> Academics() { await LoadCommon(); return View(); }

    public async Task<IActionResult> Examinations()
    {
        await LoadCommon();
        ViewBag.Notices = await _db.Notices.Where(x => x.CollegeId == CollegeId && x.IsPublished && x.Category == "Examination").OrderByDescending(x => x.PublishedOn).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Results()
    {
        await LoadCommon();
        ViewBag.Marks = await _db.Marks.Where(x => x.StudentId == StudentId).OrderBy(x => x.Subject).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Attendance()
    {
        await LoadCommon();
        ViewBag.Attendance = await _db.Attendances.Where(x => x.StudentId == StudentId).OrderByDescending(x => x.AttendanceDate).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Notices()
    {
        await LoadCommon();
        ViewBag.Notices = await _db.Notices.Where(x => x.CollegeId == CollegeId && x.IsPublished).OrderByDescending(x => x.PublishedOn).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Placements()
    {
        await LoadCommon();
        var student = await _db.Users.FindAsync(StudentId);
        ViewBag.Eligible = student != null && student.Semester >= 7;
        ViewBag.PlacementNotices = await _db.Notices.Where(x => x.CollegeId == CollegeId && x.IsPublished && x.Category == "Placement").OrderByDescending(x => x.PublishedOn).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Library()
    {
        await LoadCommon();
        ViewBag.Loans = await _db.LibraryLoans.Where(x => x.StudentId == StudentId).OrderByDescending(x => x.IssuedOn).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Hostel() { await LoadCommon(); return View(); }

    public async Task<IActionResult> FeePayments()
    {
        await LoadCommon();
        ViewBag.Fees = await _db.FeePayments.Where(x => x.StudentId == StudentId).OrderByDescending(x => x.DueDate).ToListAsync();
        return View();
    }

    public async Task<IActionResult> AcademicCalendar()
    {
        await LoadCommon();
        ViewBag.Calendar = await _db.AcademicCalendarEntries.Where(x => x.CollegeId == CollegeId).OrderBy(x => x.StartDate).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Downloads()
    {
        await LoadCommon();
        ViewBag.Downloads = await _db.DownloadRecords.Where(x => x.StudentId == StudentId).OrderByDescending(x => x.DownloadedOn).ToListAsync();
        return View();
    }
}
