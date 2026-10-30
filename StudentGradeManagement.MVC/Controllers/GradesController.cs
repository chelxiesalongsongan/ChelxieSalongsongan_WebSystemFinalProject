
using Microsoft.AspNetCore.Mvc;
using StudentGradeManagement.MVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StudentGradeManagement.MVC.Controllers
{
    public class GradesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GradesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Display all grades for Faculty.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsFacultyLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            HttpClient client =
                _httpClientFactory.CreateClient(
                    "StudentGradeManagementAPI");

            SetAuthorizationHeader(client);

            try
            {
                HttpResponseMessage response =
                    await client.GetAsync("/api/Grades");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return Content(
                        "Access denied. Only Faculty can view all grades.");
                }

                response.EnsureSuccessStatusCode();

                List<GradeViewModel>? grades =
                    await response.Content
                        .ReadFromJsonAsync<List<GradeViewModel>>();

                return View(grades ?? new List<GradeViewModel>());
            }
            catch (HttpRequestException)
            {
                return Content(
                    "Cannot connect to the API. Please check if it is running.");
            }
        }

        // Display the Add Grade form.
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsFacultyLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new GradeCreateViewModel());
        }

        // Submit the new grade to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GradeCreateViewModel model)
        {
            if (!IsFacultyLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpClient client =
                _httpClientFactory.CreateClient(
                    "StudentGradeManagementAPI");

            SetAuthorizationHeader(client);

            try
            {
                HttpResponseMessage response =
                    await client.PostAsJsonAsync("/api/Grades", model);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Access denied. Only Faculty can add grades.");

                    return View(model);
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Grade added successfully.";

                    return RedirectToAction(nameof(Index));
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "A grade record already exists for this student, subject, semester, and school year.");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to save the grade. Please check your entries and try again.");
                }

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot connect to the API. Please check if it is running.");

                return View(model);
            }
        }

        // Display an existing grade in the Edit form.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsFacultyLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            HttpClient client =
                _httpClientFactory.CreateClient(
                    "StudentGradeManagementAPI");

            SetAuthorizationHeader(client);

            try
            {
                HttpResponseMessage response =
                    await client.GetAsync($"/api/Grades/{id}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Grade record not found.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return Content(
                        "Unable to retrieve the grade record.");
                }

                GradeViewModel? grade =
                    await response.Content
                        .ReadFromJsonAsync<GradeViewModel>();

                if (grade == null)
                {
                    return NotFound("Grade record not found.");
                }

                GradeEditViewModel model = new GradeEditViewModel
                {
                    GradeId = grade.GradeId,
                    StudentId = grade.StudentId,
                    SubjectCode = grade.SubjectCode,
                    SubjectName = grade.SubjectName,
                    Grade = grade.Grade,
                    Semester = grade.Semester,
                    SchoolYear = grade.SchoolYear
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                return Content(
                    "Cannot connect to the API. Please check if it is running.");
            }
        }

        // Submit the updated grade to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, GradeEditViewModel model)
        {
            if (!IsFacultyLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.GradeId)
            {
                return BadRequest("Invalid grade ID.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpClient client =
                _httpClientFactory.CreateClient(
                    "StudentGradeManagementAPI");

            SetAuthorizationHeader(client);

            try
            {
                HttpResponseMessage response =
                    await client.PutAsJsonAsync(
                        $"/api/Grades/{id}",
                        model);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Access denied. Only Faculty can update grades.");

                    return View(model);
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Grade record not found.");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "This update conflicts with an existing grade record.");

                    return View(model);
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Grade updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update the grade. Please check your entries and try again.");

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot connect to the API. Please check if it is running.");

                return View(model);
            }
        }

        // Display the Delete confirmation page.
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsFacultyLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            HttpClient client =
                _httpClientFactory.CreateClient(
                    "StudentGradeManagementAPI");

            SetAuthorizationHeader(client);

            try
            {
                HttpResponseMessage response =
                    await client.GetAsync($"/api/Grades/{id}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Grade record not found.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return Content(
                        "Unable to retrieve the grade record.");
                }

                GradeViewModel? grade =
                    await response.Content
                        .ReadFromJsonAsync<GradeViewModel>();

                if (grade == null)
                {
                    return NotFound("Grade record not found.");
                }

                return View(grade);
            }
            catch (HttpRequestException)
            {
                return Content(
                    "Cannot connect to the API. Please check if it is running.");
            }
        }

        // Delete the grade after confirmation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsFacultyLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            HttpClient client =
                _httpClientFactory.CreateClient(
                    "StudentGradeManagementAPI");

            SetAuthorizationHeader(client);

            try
            {
                HttpResponseMessage response =
                    await client.DeleteAsync($"/api/Grades/{id}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return Content(
                        "Access denied. Only Faculty can delete grades.");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Grade record not found.");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Grade deleted successfully.";

                    return RedirectToAction(nameof(Index));
                }

                return Content(
                    "Unable to delete the grade. Please try again.");
            }
            catch (HttpRequestException)
            {
                return Content(
                    "Cannot connect to the API. Please check if it is running.");
            }
        }

        // Check whether the current user is Faculty.
        private bool IsFacultyLoggedIn()
        {
            string? token =
                HttpContext.Session.GetString("JwtToken");

            string? role =
                HttpContext.Session.GetString("UserRole");

            return !string.IsNullOrWhiteSpace(token) &&
                   string.Equals(
                       role,
                       "Faculty",
                       StringComparison.OrdinalIgnoreCase);
        }

        // Attach the current Faculty JWT token to an API request.
        private void SetAuthorizationHeader(HttpClient client)
        {
            string? token =
                HttpContext.Session.GetString("JwtToken");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
