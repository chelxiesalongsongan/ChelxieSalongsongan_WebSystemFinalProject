
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using StudentGradeManagement.API.Data;
using StudentGradeManagement.API.Models;
using System.Security.Claims;

namespace StudentGradeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Computes remarks on the server.
        private static string GetRemarks(int grade)
        {
            if (grade >= 95) return "CMS";
            if (grade >= 90) return "SP";
            if (grade >= 85) return "SAP";
            if (grade >= 80) return "GP";
            if (grade >= 75) return "EP";

            return "BEP/FAILED";
        }

        // Returns a clean grade response without circular references.
        private static object ToGradeResponse(StudentGrade g)
        {
            return new
            {
                g.GradeId,
                g.StudentId,
                g.SubjectCode,
                g.SubjectName,
                g.FacultyId,
                g.Grade,
                g.Remarks,
                g.Semester,
                g.SchoolYear,
                g.DatePosted,
                g.DateModified,
                Student = g.Student == null ? null : new
                {
                    g.Student.StudentId,
                    g.Student.StudentNumber,
                    g.Student.FirstName,
                    g.Student.LastName,
                    g.Student.Email,
                    g.Student.IsActive
                },
                Faculty = g.Faculty == null ? null : new
                {
                    g.Faculty.FacultyId,
                    g.Faculty.FacultyNumber,
                    g.Faculty.FirstName,
                    g.Faculty.LastName,
                    g.Faculty.Email,
                    g.Faculty.IsActive
                }
            };
        }

        // GET: api/grades
        // Faculty can view all grades.
        [HttpGet]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> GetGrades()
        {
            List<StudentGrade> grades = await _context.StudentGrades
                .AsNoTracking()
                .Include(g => g.Student)
                .Include(g => g.Faculty)
                .OrderBy(g => g.GradeId)
                .ToListAsync();

            return Ok(grades.Select(ToGradeResponse));
        }

        // GET: api/grades/1
        // Faculty can view a specific grade record.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> GetGrade(int id)
        {
            StudentGrade? grade = await _context.StudentGrades
                .AsNoTracking()
                .Include(g => g.Student)
                .Include(g => g.Faculty)
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
            {
                return NotFound(new
                {
                    message = "Grade record not found."
                });
            }

            return Ok(ToGradeResponse(grade));
        }

        // GET: api/grades/my-grades
        // Students can view only grades linked to their own StudentId.
        [HttpGet("my-grades")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyGrades()
        {
            string? studentIdClaim = User.FindFirst("StudentId")?.Value;

            if (!int.TryParse(studentIdClaim, out int studentId))
            {
                return Forbid();
            }

            List<StudentGrade> grades = await _context.StudentGrades
                .AsNoTracking()
                .Include(g => g.Student)
                .Include(g => g.Faculty)
                .Where(g => g.StudentId == studentId)
                .OrderBy(g => g.GradeId)
                .ToListAsync();

            return Ok(grades.Select(ToGradeResponse));
        }

        // POST: api/grades
        // Only Faculty can create grades.
        [HttpPost]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> CreateGrade(
            [FromBody] GradeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool studentExists = await _context.Students
                .AnyAsync(s => s.StudentId == request.StudentId);

            if (!studentExists)
            {
                return BadRequest(new
                {
                    message = "Student does not exist."
                });
            }

            // Get FacultyId from the authenticated user's token.
            string? facultyIdClaim = User.FindFirst("FacultyId")?.Value;

            if (!int.TryParse(facultyIdClaim, out int facultyId))
            {
                return Forbid();
            }

            bool facultyExists = await _context.Faculty
                .AnyAsync(f => f.FacultyId == facultyId);

            if (!facultyExists)
            {
                return Forbid();
            }

            // Check for an existing grade for the same student,
            // subject, semester, and school year.
            bool duplicateGrade = await _context.StudentGrades
                .AnyAsync(g =>
                    g.StudentId == request.StudentId &&
                    g.SubjectCode == request.SubjectCode &&
                    g.Semester == request.Semester &&
                    g.SchoolYear == request.SchoolYear);

            if (duplicateGrade)
            {
                return Conflict(new
                {
                    message = "A grade record already exists for this student, subject, semester, and school year."
                });
            }

            StudentGrade grade = new StudentGrade
            {
                StudentId = request.StudentId,
                SubjectCode = request.SubjectCode,
                SubjectName = request.SubjectName,
                FacultyId = facultyId,
                Grade = request.Grade,
                Remarks = GetRemarks(request.Grade),
                Semester = request.Semester,
                SchoolYear = request.SchoolYear,
                DatePosted = DateTime.Now
            };

            _context.StudentGrades.Add(grade);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.GetBaseException() is SqlException sqlException
                    && (sqlException.Number == 2601
                        || sqlException.Number == 2627))
            {
                // Handle a duplicate-key error without exposing SQL details.
                return Conflict(new
                {
                    message = "A grade record already exists for this student, subject, semester, and school year."
                });
            }

            StudentGrade? savedGrade = await _context.StudentGrades
                .AsNoTracking()
                .Include(g => g.Student)
                .Include(g => g.Faculty)
                .FirstOrDefaultAsync(g => g.GradeId == grade.GradeId);

            if (savedGrade == null)
            {
                return StatusCode(500, new
                {
                    message = "The grade was saved, but the response could not be retrieved."
                });
            }

            return CreatedAtAction(
                nameof(GetGrade),
                new { id = grade.GradeId },
                ToGradeResponse(savedGrade));
        }

        // PUT: api/grades/1
        // Only Faculty can update grades.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> UpdateGrade(
            int id,
            [FromBody] GradeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StudentGrade? grade = await _context.StudentGrades
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
            {
                return NotFound(new
                {
                    message = "Grade record not found."
                });
            }

            bool studentExists = await _context.Students
                .AnyAsync(s => s.StudentId == request.StudentId);

            if (!studentExists)
            {
                return BadRequest(new
                {
                    message = "Student does not exist."
                });
            }

            string? facultyIdClaim = User.FindFirst("FacultyId")?.Value;

            if (!int.TryParse(facultyIdClaim, out int facultyId))
            {
                return Forbid();
            }

            bool facultyExists = await _context.Faculty
                .AnyAsync(f => f.FacultyId == facultyId);

            if (!facultyExists)
            {
                return Forbid();
            }

            grade.StudentId = request.StudentId;
            grade.SubjectCode = request.SubjectCode;
            grade.SubjectName = request.SubjectName;
            grade.FacultyId = facultyId;
            grade.Grade = request.Grade;

            // Remarks are always calculated by the server.
            grade.Remarks = GetRemarks(request.Grade);

            grade.Semester = request.Semester;
            grade.SchoolYear = request.SchoolYear;
            grade.DateModified = DateTime.Now;

            await _context.SaveChangesAsync();

            StudentGrade? updatedGrade = await _context.StudentGrades
                .AsNoTracking()
                .Include(g => g.Student)
                .Include(g => g.Faculty)
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (updatedGrade == null)
            {
                return NotFound(new
                {
                    message = "Grade record not found after update."
                });
            }

            return Ok(ToGradeResponse(updatedGrade));
        }

        // DELETE: api/grades/1
        // Only Faculty can delete grades.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            StudentGrade? grade = await _context.StudentGrades
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
            {
                return NotFound(new
                {
                    message = "Grade record not found."
                });
            }

            _context.StudentGrades.Remove(grade);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Grade record deleted successfully."
            });
        }
    }
}
