using Microsoft.AspNetCore.Identity;

namespace StudentGradeManagement.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Links this login account to an existing student record.
        public int? StudentId { get; set; }

        // Links this login account to an existing faculty record.
        public int? FacultyId { get; set; }
    }
}