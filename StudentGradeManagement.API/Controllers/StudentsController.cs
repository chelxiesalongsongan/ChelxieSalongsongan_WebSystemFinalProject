
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Data;
using StudentGradeManagement.API.Models;

namespace StudentGradeManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Students
        // Faculty can view all student records.
        [HttpGet]
        [Authorize(Roles = "Faculty")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            List<Student> students = await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            return Ok(students);
        }

        // GET: api/Students/5
        // Faculty can view one student record.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Faculty")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            Student? student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return NotFound(new
                {
                    message = "Student record not found."
                });
            }

            return Ok(student);
        }

        // GET: api/Students/my-profile
        // Students can view only their own profile.
        [HttpGet("my-profile")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<StudentProfileResponse>> GetMyProfile()
        {
            string? studentIdClaim =
                User.FindFirst("StudentId")?.Value;

            if (!int.TryParse(studentIdClaim, out int studentId))
            {
                return Unauthorized(new
                {
                    message = "Student ID was not found in your account."
                });
            }

            Student? student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                return NotFound(new
                {
                    message = "Student record not found."
                });
            }

            StudentProfileResponse profile = new StudentProfileResponse
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                FullName = student.FirstName + " " + student.LastName,
                StudentNumber = student.StudentNumber
            };

            return Ok(profile);
        }
    }

    // Response returned to the logged-in student.
    public class StudentProfileResponse
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string StudentNumber { get; set; } = string.Empty;
    }
}
