
using Microsoft.AspNetCore.Mvc;

namespace StudentGradeManagement.MVC.Controllers
{
    public class FacultyController : Controller
    {
        public IActionResult Index()
        {
            // Check if the user is logged in as Faculty.
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || role != "Faculty")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}
