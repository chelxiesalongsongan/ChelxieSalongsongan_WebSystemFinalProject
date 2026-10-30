
using System.ComponentModel.DataAnnotations;

namespace StudentGradeManagement.API.Models
{
    public class GradeRequest
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [Required]
        public int FacultyId { get; set; }

        [Required]
        [Range(0, 100)]
        public int Grade { get; set; }

        [Required]
        [StringLength(50)]
        public string Semester { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SchoolYear { get; set; } = string.Empty;
    }
}
