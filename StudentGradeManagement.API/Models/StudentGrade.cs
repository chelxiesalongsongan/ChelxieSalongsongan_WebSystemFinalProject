
using System.ComponentModel.DataAnnotations;

namespace StudentGradeManagement.API.Models
{
    public class StudentGrade
    {
        public int GradeId { get; set; }

        public int StudentId { get; set; }

        [Required]
        [StringLength(30)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string SubjectName { get; set; } = string.Empty;

        public int FacultyId { get; set; }

        [Range(0, 100)]
        public int Grade { get; set; }

        // The API will generate this value.
        [Required]
        [StringLength(30)]
        public string Remarks { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Semester { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SchoolYear { get; set; } = string.Empty;

        public DateTime DatePosted { get; set; }

        public DateTime? DateModified { get; set; }

        // Relationships to the existing database tables.
        public Student Student { get; set; } = null!;

        public Faculty Faculty { get; set; } = null!;
    }
}
