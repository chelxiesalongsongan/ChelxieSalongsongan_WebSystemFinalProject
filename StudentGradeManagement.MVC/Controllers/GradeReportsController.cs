
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StudentGradeManagement.MVC.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StudentGradeManagement.MVC.Controllers
{
    public class GradeReportsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;

        public GradeReportsController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }

        // Display grade reports
        public async Task<IActionResult> Index()
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || role != "Faculty")
            {
                return RedirectToAction("Login", "Account");
            }

            List<GradeReportViewModel>? reports =
                await GetGradeReports(token);

            if (reports == null)
            {
                ViewBag.ErrorMessage =
                    "Unable to load grade reports. Please try again.";

                return View(new List<GradeReportViewModel>());
            }

            return View(reports);
        }

        // Export filtered grade reports to CSV
        public async Task<IActionResult> ExportCsv(
            string search = "",
            string semester = "",
            string schoolYear = "")
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || role != "Faculty")
            {
                return RedirectToAction("Login", "Account");
            }

            List<GradeReportViewModel>? reports =
                await GetGradeReports(token);

            if (reports == null)
            {
                TempData["ExportError"] =
                    "Unable to export grade reports. Please try again.";

                return RedirectToAction("Index");
            }

            reports = FilterReports(
                reports, search, semester, schoolYear);

            if (reports.Count == 0)
            {
                TempData["ExportError"] =
                    "No matching grade records to export.";

                return RedirectToAction("Index");
            }

            StringBuilder csv = new StringBuilder();

            csv.AppendLine(
                "Student Number,Student Name,Subject,Semester,School Year,Grade,Remarks");

            foreach (GradeReportViewModel report in reports)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(report.StudentNumber),
                    EscapeCsv(report.StudentName),
                    EscapeCsv(report.SubjectCode),
                    EscapeCsv(report.Semester),
                    EscapeCsv(report.SchoolYear),
                    EscapeCsv(report.Grade.ToString("0.##")),
                    EscapeCsv(report.Remarks)
                ));
            }

            byte[] fileBytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
                .ToArray();

            string fileName =
                $"GradeReports_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(
                fileBytes,
                "text/csv; charset=utf-8",
                fileName);
        }

        // Export filtered grade reports to PDF
        public async Task<IActionResult> ExportPdf(
            string search = "",
            string semester = "",
            string schoolYear = "")
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || role != "Faculty")
            {
                return RedirectToAction("Login", "Account");
            }

            List<GradeReportViewModel>? reports =
                await GetGradeReports(token);

            if (reports == null)
            {
                TempData["ExportError"] =
                    "Unable to export grade reports. Please try again.";

                return RedirectToAction("Index");
            }

            reports = FilterReports(
                reports, search, semester, schoolYear);

            if (reports.Count == 0)
            {
                TempData["ExportError"] =
                    "No matching grade records to export.";

                return RedirectToAction("Index");
            }

            QuestPDF.Settings.License = LicenseType.Community;

            // Locate the UST Angelicum logo.
            // Actual file: wwwroot/images/uac_logo.png
            string logoPath = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "images",
                "uac_logo.png");

            byte[]? logoBytes = null;

            if (System.IO.File.Exists(logoPath))
            {
                logoBytes =
                    await System.IO.File.ReadAllBytesAsync(logoPath);
            }

            byte[] pdfBytes = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.DefaultTextStyle(style =>
                        style.FontSize(9));

                    // PDF Header
                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            // Display logo when the file is found.
                            if (logoBytes != null)
                            {
                                row.ConstantItem(65)
                                    .Height(65)
                                    .AlignMiddle()
                                    .Image(logoBytes)
                                    .FitArea();
                            }

                            row.RelativeItem()
                                .PaddingLeft(12)
                                .AlignMiddle()
                                .Column(column =>
                                {
                                    column.Item()
                                        .Text("UST ANGELICUM COLLEGE")
                                        .FontSize(17)
                                        .Bold()
                                        .FontColor("#164A8A");

                                    column.Item()
                                        .Text("STUDENT GRADE MANAGEMENT SYSTEM")
                                        .FontSize(10)
                                        .FontColor("#164A8A");

                                    column.Item()
                                        .Text("GRADE REPORTS")
                                        .FontSize(13)
                                        .Bold();
                                });
                        });

                        header.Item()
                            .PaddingTop(8)
                            .LineHorizontal(2)
                            .LineColor("#164A8A");

                        header.Item()
                            .PaddingTop(8)
                            .Row(row =>
                            {
                                row.RelativeItem().Text(
                                    $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}");

                                row.RelativeItem()
                                    .AlignRight()
                                    .Text($"Records: {reports.Count}");
                            });

                        // Display active filters in the PDF.
                        if (!string.IsNullOrWhiteSpace(search) ||
                            !string.IsNullOrWhiteSpace(semester) ||
                            !string.IsNullOrWhiteSpace(schoolYear))
                        {
                            header.Item()
                                .PaddingTop(4)
                                .Text(text =>
                                {
                                    text.Span("Filters: ").Bold();

                                    if (!string.IsNullOrWhiteSpace(search))
                                    {
                                        text.Span($"Search = {search}; ");
                                    }

                                    if (!string.IsNullOrWhiteSpace(semester))
                                    {
                                        text.Span($"Semester = {semester}; ");
                                    }

                                    if (!string.IsNullOrWhiteSpace(schoolYear))
                                    {
                                        text.Span($"School Year = {schoolYear}");
                                    }
                                });
                        }
                    });

                    // PDF Table
                    page.Content()
                        .PaddingTop(15)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.8f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(0.7f);
                                columns.RelativeColumn(1.4f);
                            });

                            table.Header(header =>
                            {
                                HeaderCell(header, "Student No.");
                                HeaderCell(header, "Student Name");
                                HeaderCell(header, "Subject");
                                HeaderCell(header, "Semester");
                                HeaderCell(header, "School Year");
                                HeaderCell(header, "Grade");
                                HeaderCell(header, "Remarks");
                            });

                            foreach (GradeReportViewModel report in reports)
                            {
                                BodyCell(table, report.StudentNumber);
                                BodyCell(table, report.StudentName);
                                BodyCell(table, report.SubjectCode);
                                BodyCell(table, report.Semester);
                                BodyCell(table, report.SchoolYear);
                                BodyCell(table, report.Grade.ToString("0.##"));
                                BodyCell(table, report.Remarks);
                            }
                        });

                    // PDF Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "UST Angelicum College | Grade Reports | ");

                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();

            string fileName =
                $"GradeReports_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                fileName);
        }

        // Retrieve grade reports from the API
        private async Task<List<GradeReportViewModel>?> GetGradeReports(
            string token)
        {
            HttpClient client = _httpClientFactory.CreateClient(
                "StudentGradeManagementAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response =
                await client.GetAsync("/api/Grades");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<
                List<GradeReportViewModel>>();
        }

        // Apply search and filters to CSV and PDF exports
        private static List<GradeReportViewModel> FilterReports(
            List<GradeReportViewModel> reports,
            string search,
            string semester,
            string schoolYear)
        {
            string searchText = search.Trim();
            string selectedSemester = semester.Trim();
            string selectedSchoolYear = schoolYear.Trim();

            return reports.Where(report =>
            {
                string rowText = string.Join(" ",
                    report.StudentNumber,
                    report.StudentName,
                    report.SubjectCode,
                    report.SubjectName,
                    report.Semester,
                    report.SchoolYear,
                    report.Grade.ToString("0.##"),
                    report.Remarks);

                bool matchesSearch =
                    string.IsNullOrWhiteSpace(searchText) ||
                    rowText.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase);

                bool matchesSemester =
                    string.IsNullOrWhiteSpace(selectedSemester) ||
                    string.Equals(
                        report.Semester,
                        selectedSemester,
                        StringComparison.OrdinalIgnoreCase);

                bool matchesSchoolYear =
                    string.IsNullOrWhiteSpace(selectedSchoolYear) ||
                    string.Equals(
                        report.SchoolYear,
                        selectedSchoolYear,
                        StringComparison.OrdinalIgnoreCase);

                return matchesSearch &&
                       matchesSemester &&
                       matchesSchoolYear;
            }).ToList();
        }

        // CSV formatting
        private static string EscapeCsv(string? value)
        {
            value ??= string.Empty;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        // PDF table header styling
        private static void HeaderCell(
            TableCellDescriptor table,
            string text)
        {
            table.Cell()
                .Background("#164A8A")
                .Padding(6)
                .Text(text)
                .FontColor("#FFFFFF")
                .Bold()
                .FontSize(8);
        }

        // PDF table body styling
        private static void BodyCell(
            TableDescriptor table,
            string? value)
        {
            table.Cell()
                .BorderBottom(0.5f)
                .BorderColor("#D6E2F0")
                .Padding(5)
                .Text(string.IsNullOrWhiteSpace(value) ? "—" : value)
                .FontSize(8);
        }
    }
}
