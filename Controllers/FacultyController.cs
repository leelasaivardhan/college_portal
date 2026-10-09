using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiCollegePortal.Data;
using MultiCollegePortal.Models;

namespace MultiCollegePortal.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyController : Controller
{
    private readonly PortalDbContext _db;
    public FacultyController(PortalDbContext db) => _db = db;
    private int FacultyId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int CollegeId => int.Parse(User.FindFirstValue("CollegeId")!);

    private async Task LoadCommon()
    {
        ViewBag.College = await _db.Colleges.FindAsync(CollegeId);
        ViewBag.Faculty = await _db.Users.FindAsync(FacultyId);
    }

    private async Task LoadEntryLists()
    {
        ViewBag.Students = await _db.Users.Where(x => x.CollegeId == CollegeId && x.Role == UserRole.Student && x.IsActive).OrderBy(x => x.RollNumber).ToListAsync();
        ViewBag.Subjects = await _db.Periods.Where(x => x.CollegeId == CollegeId && x.FacultyId == FacultyId).Select(x => x.Subject).Distinct().OrderBy(x => x).ToListAsync();
    }

    public async Task<IActionResult> Dashboard()
    {
        await LoadCommon();
        var periods = await _db.Periods.Where(x => x.CollegeId == CollegeId && x.FacultyId == FacultyId).OrderBy(x => x.Day).ThenBy(x => x.TimeSlot).ToListAsync();
        ViewBag.Periods = periods;
        ViewBag.Students = await _db.Users.CountAsync(x => x.CollegeId == CollegeId && x.Role == UserRole.Student && x.IsActive);
        ViewBag.SubjectCount = periods.Select(x => x.Subject).Distinct().Count();
        var todaySubjects = periods.Where(x => x.Day == DateTime.Today.DayOfWeek).Select(x => x.Subject).Distinct().ToList();
        var todayStudents = await _db.Users.Where(x => x.CollegeId == CollegeId && x.Role == UserRole.Student && x.IsActive).Select(x => x.Id).ToListAsync();
        ViewBag.PendingActions = await _db.Attendances.CountAsync(x => x.FacultyId == FacultyId && x.AttendanceDate.Date == DateTime.Today) == 0 && todaySubjects.Count > 0 ? todaySubjects.Count : 0;
        return View();
    }

    public async Task<IActionResult> PendingActions()
    {
        await LoadCommon();
        ViewBag.Periods = await _db.Periods.Where(x => x.CollegeId == CollegeId && x.FacultyId == FacultyId).OrderBy(x => x.Day).ThenBy(x => x.TimeSlot).ToListAsync();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> AddMarks()
    {
        await LoadCommon(); await LoadEntryLists();
        return View(new MarkEntryViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMarks(MarkEntryViewModel model)
    {
        await LoadCommon(); await LoadEntryLists();
        if (!ModelState.IsValid) return View(model);
        if (model.Score > 30) { ModelState.AddModelError(nameof(model.Score), "Maximum marks for Mid1-Mid4 and Lab1-Lab2 is 30."); return View(model); }
        var student = await _db.Users.FirstOrDefaultAsync(x => x.Id == model.StudentId && x.CollegeId == CollegeId && x.Role == UserRole.Student);
        var subjectAllowed = await _db.Periods.AnyAsync(x => x.CollegeId == CollegeId && x.FacultyId == FacultyId && x.Subject == model.Subject);
        if (student == null || !subjectAllowed) { ModelState.AddModelError("", "Please select a valid student and a subject assigned to you."); return View(model); }
        var existing = await _db.Marks.FirstOrDefaultAsync(x => x.StudentId == model.StudentId && x.Subject == model.Subject && x.Assessment == model.Assessment);
        if (existing == null)
        {
            _db.Marks.Add(new Mark { CollegeId = CollegeId, FacultyId = FacultyId, StudentId = model.StudentId, Subject = model.Subject, Assessment = model.Assessment, Score = model.Score, MaxScore = 30 });
        }
        else
        {
            existing.Score = model.Score; existing.FacultyId = FacultyId; existing.UpdatedOn = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Successfully added the marks.";
        return RedirectToAction(nameof(AddMarks));
    }

    [HttpGet]
    public async Task<IActionResult> Attendance()
    {
        await LoadCommon(); await LoadEntryLists();
        return View(new AttendanceEntryViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(AttendanceEntryViewModel model)
    {
        await LoadCommon(); await LoadEntryLists();
        if (!ModelState.IsValid) return View(model);
        var student = await _db.Users.FirstOrDefaultAsync(x => x.Id == model.StudentId && x.CollegeId == CollegeId && x.Role == UserRole.Student);
        var subjectAllowed = await _db.Periods.AnyAsync(x => x.CollegeId == CollegeId && x.FacultyId == FacultyId && x.Subject == model.Subject);
        if (student == null || !subjectAllowed) { ModelState.AddModelError("", "Please select a valid student and a subject assigned to you."); return View(model); }
        var existing = await _db.Attendances.FirstOrDefaultAsync(x => x.StudentId == model.StudentId && x.Subject == model.Subject && x.AttendanceDate.Date == model.AttendanceDate.Date);
        if (existing == null)
            _db.Attendances.Add(new Attendance { CollegeId = CollegeId, FacultyId = FacultyId, StudentId = model.StudentId, AttendanceDate = model.AttendanceDate.Date, Subject = model.Subject, Present = model.Present });
        else { existing.Present = model.Present; existing.FacultyId = FacultyId; }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Successfully added the attendance.";
        return RedirectToAction(nameof(Attendance));
    }

    public async Task<IActionResult> MyPeriods() { await LoadCommon(); ViewBag.Periods = await _db.Periods.Where(x => x.CollegeId == CollegeId && x.FacultyId == FacultyId).OrderBy(x => x.Day).ThenBy(x => x.TimeSlot).ToListAsync(); return View(); }
    public async Task<IActionResult> StudentList() { await LoadCommon(); ViewBag.Students = await _db.Users.Where(x => x.CollegeId == CollegeId && x.Role == UserRole.Student).OrderBy(x => x.RollNumber).ToListAsync(); return View(); }
    public async Task<IActionResult> Notices() { await LoadCommon(); ViewBag.Notices = await _db.Notices.Where(x => x.CollegeId == CollegeId && x.IsPublished).OrderByDescending(x => x.PublishedOn).ToListAsync(); return View(); }
    public async Task<IActionResult> Examination() { await LoadCommon(); ViewBag.Notices = await _db.Notices.Where(x => x.CollegeId == CollegeId && x.IsPublished && x.Category == "Examination").OrderByDescending(x => x.PublishedOn).ToListAsync(); return View(); }
    public async Task<IActionResult> Library() { await LoadCommon(); ViewBag.Loans = await _db.LibraryLoans.Where(x => x.CollegeId == CollegeId).OrderByDescending(x => x.IssuedOn).Take(50).ToListAsync(); return View(); }
}
