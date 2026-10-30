
namespace StudentGradeManagement.MVC.Models
{
    public class StudentViewModel
    {
        public int StudentId { get; set; }

        public string StudentNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
