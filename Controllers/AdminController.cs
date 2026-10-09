using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiCollegePortal.Data;
using MultiCollegePortal.Models;

namespace MultiCollegePortal.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly PortalDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher = new();
    public AdminController(PortalDbContext db) => _db = db;

    private int CollegeId => int.TryParse(User.FindFirstValue("CollegeId"), out var id) ? id : 0;

    private async Task<int> EnsureActiveCollege()
    {
        if (CollegeId != 0 && await _db.Colleges.AnyAsync(x => x.Id == CollegeId && x.IsActive)) return CollegeId;
        return await _db.Colleges.Where(x => x.IsActive).Select(x => x.Id).FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Dashboard()
    {
        var activeId = await EnsureActiveCollege();
        var colleges = await _db.Colleges.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
        ViewBag.Colleges = colleges; ViewBag.ActiveCollegeId = activeId; ViewBag.College = await _db.Colleges.FindAsync(activeId);
        ViewBag.Students = await _db.Users.CountAsync(x => x.CollegeId == activeId && x.Role == UserRole.Student);
        ViewBag.Faculty = await _db.Users.CountAsync(x => x.CollegeId == activeId && x.Role == UserRole.Faculty);
        ViewBag.Notices = await _db.Notices.CountAsync(x => x.CollegeId == activeId);
        ViewBag.Periods = await _db.Periods.CountAsync(x => x.CollegeId == activeId);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchCollege(int collegeId)
    {
        if (!await _db.Colleges.AnyAsync(x => x.Id == collegeId && x.IsActive)) return NotFound();
        var claims = User.Claims.Where(c => c.Type != "CollegeId").ToList();
        claims.Add(new Claim("CollegeId", collegeId.ToString()));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> CollegeManagement() { ViewBag.Colleges = await _db.Colleges.OrderBy(x => x.Name).ToListAsync(); return View(); }

    public async Task<IActionResult> ManageContent() => RedirectToAction(nameof(Notices));

    [HttpGet] public async Task<IActionResult> Notices() => View(await _db.Notices.Where(x => x.CollegeId == CollegeId).OrderByDescending(x => x.PublishedOn).ToListAsync());

    [HttpGet] public IActionResult AddNotice() => View(new Notice { IsPublished = true, Category = "General" });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNotice(Notice model)
    {
        if (CollegeId == 0) { ModelState.AddModelError("", "Please select a college first."); return View(model); }
        if (!ModelState.IsValid) return View(model);
        model.CollegeId = CollegeId; model.PublishedOn = DateTime.UtcNow;
        _db.Notices.Add(model); await _db.SaveChangesAsync();
        TempData["Success"] = "Notice published successfully.";
        return RedirectToAction(nameof(Notices));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNotice(int id)
    {
        var item = await _db.Notices.FirstOrDefaultAsync(x => x.Id == id && x.CollegeId == CollegeId);
        if (item != null) { _db.Notices.Remove(item); await _db.SaveChangesAsync(); TempData["Success"] = "Notice removed successfully."; }
        return RedirectToAction(nameof(Notices));
    }

    [HttpGet]
    public async Task<IActionResult> AddPeriod()
    {
        ViewBag.Faculty = await _db.Users.Where(x => x.CollegeId == CollegeId && x.Role == UserRole.Faculty && x.IsActive).OrderBy(x => x.FullName).ToListAsync();
        return View(new Period { WeekDate = DateTime.Today, Day = DateTime.Today.DayOfWeek });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPeriod(Period model)
    {
        ViewBag.Faculty = await _db.Users.Where(x => x.CollegeId == CollegeId && x.Role == UserRole.Faculty && x.IsActive).OrderBy(x => x.FullName).ToListAsync();
        if (CollegeId == 0) { ModelState.AddModelError("", "Please select a college first."); return View(model); }
        if (model.FacultyId.HasValue && !await _db.Users.AnyAsync(x => x.Id == model.FacultyId && x.CollegeId == CollegeId && x.Role == UserRole.Faculty))
            ModelState.AddModelError(nameof(model.FacultyId), "Selected faculty does not belong to the active college.");
        if (!ModelState.IsValid) return View(model);
        model.CollegeId = CollegeId;
        _db.Periods.Add(model); await _db.SaveChangesAsync();
        TempData["Success"] = "Period added successfully.";
        return RedirectToAction(nameof(TimetableManagement));
    }

    public async Task<IActionResult> TimetableManagement()
    {
        ViewBag.Periods = await _db.Periods.Include(x => x.Faculty).Where(x => x.CollegeId == CollegeId).OrderBy(x => x.WeekDate).ThenBy(x => x.TimeSlot).ToListAsync();
        return View();
    }

    public async Task<IActionResult> UserManagement()
    {
        ViewBag.Users = await _db.Users.Where(x => x.CollegeId == CollegeId).OrderBy(x => x.Role).ThenBy(x => x.FullName).ToListAsync();
        return View();
    }

    [HttpGet] public IActionResult AddUser() => View(new AddUserViewModel { CollegeId = CollegeId });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(AddUserViewModel model)
    {
        model.CollegeId = CollegeId;
        if (await _db.Users.AnyAsync(x => x.Email.ToLower() == model.Email.ToLower())) ModelState.AddModelError(nameof(model.Email), "Email already exists.");
        if (!await _db.Colleges.AnyAsync(x => x.Id == CollegeId && x.IsActive)) ModelState.AddModelError("", "Please select a valid college.");
        if (!ModelState.IsValid) return View(model);
        var user = new AppUser { FullName = model.FullName, Email = model.Email.Trim().ToLowerInvariant(), Role = model.Role, CollegeId = CollegeId, RollNumber = model.RollNumber, Department = model.Department, Course = model.Course, Year = model.Year, Semester = model.Semester, IsActive = true };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);
        _db.Users.Add(user); await _db.SaveChangesAsync();
        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(UserManagement));
    }

    public async Task<IActionResult> ExaminationManagement()
    {
        ViewBag.Notices = await _db.Notices.Where(x => x.CollegeId == CollegeId && x.Category == "Examination").OrderByDescending(x => x.PublishedOn).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Reports()
    {
        ViewBag.StudentCount = await _db.Users.CountAsync(x => x.CollegeId == CollegeId && x.Role == UserRole.Student);
        ViewBag.FacultyCount = await _db.Users.CountAsync(x => x.CollegeId == CollegeId && x.Role == UserRole.Faculty);
        ViewBag.AttendanceCount = await _db.Attendances.CountAsync(x => x.CollegeId == CollegeId);
        ViewBag.MarkCount = await _db.Marks.CountAsync(x => x.CollegeId == CollegeId);
        return View();
    }
}
