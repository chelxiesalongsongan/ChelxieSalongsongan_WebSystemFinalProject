
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using StudentGradeManagement.MVC.Models;

namespace StudentGradeManagement.MVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Display student records
        public async Task<IActionResult> Index()
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("UserRole");

            // Only logged-in Faculty can view student records
            if (string.IsNullOrEmpty(token) || role != "Faculty")
            {
                return RedirectToAction("Login", "Account");
            }

            HttpClient client = _httpClientFactory.CreateClient(
                "StudentGradeManagementAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response =
                await client.GetAsync("/api/Students");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage =
                    "Unable to load student records. Please try again.";

                return View(new List<StudentViewModel>());
            }

            List<StudentViewModel>? students =
                await response.Content.ReadFromJsonAsync<List<StudentViewModel>>();

            return View(students ?? new List<StudentViewModel>());
        }
    }
}
