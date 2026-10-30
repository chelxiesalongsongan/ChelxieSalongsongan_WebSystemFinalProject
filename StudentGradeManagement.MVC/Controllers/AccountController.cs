
using Microsoft.AspNetCore.Mvc;
using StudentGradeManagement.MVC.Models;
using System.Net.Http.Json;

namespace StudentGradeManagement.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Display the Login page.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Process the Login form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpClient client =
                _httpClientFactory.CreateClient(
                    "StudentGradeManagementAPI");

            try
            {
                // Send the login credentials to the API.
                HttpResponseMessage response =
                    await client.PostAsJsonAsync(
                        "/api/Auth/login",
                        new
                        {
                            username = model.Username,
                            password = model.Password
                        });

                // Handle unsuccessful login.
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid username or password. Please try again.");

                    return View(model);
                }

                // Read the API login response.
                LoginResponse? loginResponse =
                    await response.Content
                        .ReadFromJsonAsync<LoginResponse>();

                // Check if the token was returned.
                if (loginResponse == null ||
                    string.IsNullOrWhiteSpace(loginResponse.Token))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to process the login response.");

                    return View(model);
                }

                // Get the user's role from the roles array.
                string? userRole = loginResponse.Roles.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(userRole))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your account does not have an authorized role.");

                    return View(model);
                }

                // Store the JWT token and role in the session.
                HttpContext.Session.SetString(
                    "JwtToken",
                    loginResponse.Token);

                HttpContext.Session.SetString(
                    "UserRole",
                    userRole);

                // Redirect based on the user's role.
                if (string.Equals(
                    userRole,
                    "Faculty",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Faculty");
                }

                if (string.Equals(
                    userRole,
                    "Student",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Student");
                }

                // Clear the session if the role is not authorized.
                HttpContext.Session.Clear();

                ModelState.AddModelError(
                    string.Empty,
                    "Your account does not have an authorized role.");

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

        // Log out the current user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
    }

    // Matches the JSON returned by the API.
    public class LoginResponse
    {
        public string? Message { get; set; }

        public string? UserName { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public string? Token { get; set; }
    }
}
