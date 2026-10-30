
using System.ComponentModel.DataAnnotations;

namespace StudentGradeManagement.API.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required]
        [StringLength(30)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // A student can have multiple grade records.
        public ICollection<StudentGrade> Grades { get; set; }
            = new List<StudentGrade>();
    }
}
