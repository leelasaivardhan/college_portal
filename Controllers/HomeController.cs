using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiCollegePortal.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => User.Identity?.IsAuthenticated == true ? RedirectToAction(nameof(Dashboard)) : RedirectToAction("Login", "Account");

    [Authorize]
    public IActionResult Dashboard()
    {
        if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
        if (User.IsInRole("Faculty")) return RedirectToAction("Dashboard", "Faculty");
        return RedirectToAction("Dashboard", "Student");
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}
