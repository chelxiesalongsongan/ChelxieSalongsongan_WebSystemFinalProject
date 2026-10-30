
using System.ComponentModel.DataAnnotations;

namespace StudentGradeManagement.API.Models
{
    public class Faculty
    {
        public int FacultyId { get; set; }

        [Required]
        [StringLength(30)]
        public string FacultyNumber { get; set; } = string.Empty;

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

        // Grades posted by this faculty member.
        public ICollection<StudentGrade> Grades { get; set; }
            = new List<StudentGrade>();
    }
}
