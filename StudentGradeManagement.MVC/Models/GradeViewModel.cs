
using System;

namespace StudentGradeManagement.MVC.Models
{
    public class GradeViewModel
    {
        public int GradeId { get; set; }

        public int StudentId { get; set; }

        public string SubjectCode { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public int FacultyId { get; set; }

        public int Grade { get; set; }

        public string Remarks { get; set; } = string.Empty;

        public string Semester { get; set; } = string.Empty;

        public string SchoolYear { get; set; } = string.Empty;

        public DateTime? DatePosted { get; set; }

        public DateTime? DateModified { get; set; }

        public StudentGradeStudentViewModel? Student { get; set; }

        public StudentGradeFacultyViewModel? Faculty { get; set; }
    }

    public class StudentGradeStudentViewModel
    {
        public int StudentId { get; set; }

        public string StudentNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool IsActive { get; set; }
    }

    public class StudentGradeFacultyViewModel
    {
        public int FacultyId { get; set; }

        public string FacultyNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool IsActive { get; set; }
    }
}
