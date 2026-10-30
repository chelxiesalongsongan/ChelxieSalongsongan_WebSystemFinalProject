
using System.ComponentModel.DataAnnotations;

namespace StudentGradeManagement.MVC.Models
{
    public class GradeEditViewModel
    {
        public int GradeId { get; set; }

        [Required(ErrorMessage = "Student ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a valid Student ID.")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Subject code is required.")]
        public string SubjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject name is required.")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade is required.")]
        [Range(0, 100, ErrorMessage = "Grade must be between 0 and 100.")]
        public int? Grade { get; set; }

        [Required(ErrorMessage = "Semester is required.")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "School year is required.")]
        public string SchoolYear { get; set; } = string.Empty;
    }
}
