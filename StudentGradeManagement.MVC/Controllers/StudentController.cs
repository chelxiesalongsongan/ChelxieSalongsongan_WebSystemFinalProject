
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StudentGradeManagement.MVC.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            // Get the login session.
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("UserRole");

            // Allow only logged-in students.
            if (string.IsNullOrWhiteSpace(token) ||
                !string.Equals(
                    role,
                    "Student",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            // Pass the API URL and token to the view.
            // JavaScript fetch() will use these to request data.
            ViewBag.JwtToken = token;
            ViewBag.ApiBaseUrl = "https://localhost:7043";

            return View();
        }
    }
}
