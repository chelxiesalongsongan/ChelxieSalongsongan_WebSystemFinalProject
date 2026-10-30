
using System.Text.Json.Serialization;

namespace StudentGradeManagement.MVC.Models
{
    public class GradeReportViewModel
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

        [JsonPropertyName("Student")]
        public GradeReportStudentViewModel? Student { get; set; }

        [JsonIgnore]
        public string StudentNumber =>
            Student?.StudentNumber ?? string.Empty;

        [JsonIgnore]
        public string StudentName =>
            Student == null
                ? string.Empty
                : $"{Student.FirstName} {Student.LastName}".Trim();
    }

    public class GradeReportStudentViewModel
    {
        public int StudentId { get; set; }

        public string StudentNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
