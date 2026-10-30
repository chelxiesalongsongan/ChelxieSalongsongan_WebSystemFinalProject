
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Models;

namespace StudentGradeManagement.API.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> Faculty { get; set; }
        public DbSet<StudentGrade> StudentGrades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Identity tables first.
            base.OnModelCreating(modelBuilder);

            // Students table.
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.StudentId);

                entity.HasIndex(s => s.StudentNumber)
                    .IsUnique();

                entity.HasIndex(s => s.Email)
                    .IsUnique();
            });

            // Faculty table.
            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.HasKey(f => f.FacultyId);

                entity.HasIndex(f => f.FacultyNumber)
                    .IsUnique();

                entity.HasIndex(f => f.Email)
                    .IsUnique();
            });

            // Student grades table.
            modelBuilder.Entity<StudentGrade>(entity =>
            {
                entity.HasKey(g => g.GradeId);

                entity.ToTable(table =>
                    table.HasCheckConstraint(
                        "CK_StudentGrades_Grade",
                        "[Grade] >= 0 AND [Grade] <= 100"));

                entity.HasIndex(g => new
                {
                    g.StudentId,
                    g.SubjectCode,
                    g.Semester,
                    g.SchoolYear
                }).IsUnique();

                entity.HasOne(g => g.Student)
                    .WithMany(s => s.Grades)
                    .HasForeignKey(g => g.StudentId);

                entity.HasOne(g => g.Faculty)
                    .WithMany(f => f.Grades)
                    .HasForeignKey(g => g.FacultyId);
            });
        }
    }
}
