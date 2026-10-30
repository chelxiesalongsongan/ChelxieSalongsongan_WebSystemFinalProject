
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Models;

namespace StudentGradeManagement.API.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Faculty", "Student" };

            foreach (string role in roles)
            {
                bool roleExists =
                    await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    IdentityResult result =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        string errors = string.Join(
                            ", ",
                            result.Errors.Select(
                                error => error.Description));

                        throw new InvalidOperationException(
                            $"Failed to create role {role}: {errors}");
                    }
                }
            }
        }

        public static async Task SeedTestUsersAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            // Create or verify the Faculty test account.
            ApplicationUser? facultyUser =
                await userManager.FindByNameAsync("faculty01");

            if (facultyUser == null)
            {
                int? facultyId = await dbContext.Faculty
                    .Select(faculty => (int?)faculty.FacultyId)
                    .FirstOrDefaultAsync();

                if (facultyId.HasValue)
                {
                    facultyUser = new ApplicationUser
                    {
                        UserName = "faculty01",
                        Email = "faculty01@example.com",
                        FacultyId = facultyId.Value,
                        EmailConfirmed = true
                    };

                    IdentityResult result =
                        await userManager.CreateAsync(
                            facultyUser, "Faculty@123!");

                    if (!result.Succeeded)
                    {
                        string errors = string.Join(
                            ", ",
                            result.Errors.Select(
                                error => error.Description));

                        throw new InvalidOperationException(
                            $"Failed to create Faculty account: {errors}");
                    }
                }
            }

            if (facultyUser != null &&
                !await userManager.IsInRoleAsync(
                    facultyUser, "Faculty"))
            {
                IdentityResult roleResult =
                    await userManager.AddToRoleAsync(
                        facultyUser, "Faculty");

                if (!roleResult.Succeeded)
                {
                    string errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(
                        $"Failed to assign Faculty role: {errors}");
                }
            }

            // Create or verify the student test accounts.
            await CreateStudentAccountAsync(
                userManager, dbContext,
                1, "student01", "student01@example.com",
                "Student@123!");

            await CreateStudentAccountAsync(
                userManager, dbContext,
                2, "student02", "student02@example.com",
                "Student@123!");

            await CreateStudentAccountAsync(
                userManager, dbContext,
                3, "student03", "student03@example.com",
                "Student@123!");

            // Test account for a student with no grades.
            await CreateStudentAccountAsync(
                userManager, dbContext,
                4, "student04", "teststudent04@example.com",
                "Student@123!");
        }

        private static async Task CreateStudentAccountAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            int studentId,
            string username,
            string email,
            string password)
        {
            // Make sure the corresponding student record exists.
            bool studentExists = await dbContext.Students
                .AnyAsync(student =>
                    student.StudentId == studentId);

            if (!studentExists)
            {
                return;
            }

            ApplicationUser? studentUser =
                await userManager.FindByNameAsync(username);

            // Create the account only if it does not exist.
            if (studentUser == null)
            {
                studentUser = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    StudentId = studentId,
                    EmailConfirmed = true
                };

                IdentityResult result =
                    await userManager.CreateAsync(
                        studentUser, password);

                if (!result.Succeeded)
                {
                    string errors = string.Join(
                        ", ",
                        result.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(
                        $"Failed to create account {username}: {errors}");
                }
            }
            else if (studentUser.StudentId != studentId)
            {
                throw new InvalidOperationException(
                    $"Account {username} is linked to a different student.");
            }

            // Ensure the account has the Student role.
            if (!await userManager.IsInRoleAsync(
                studentUser, "Student"))
            {
                IdentityResult roleResult =
                    await userManager.AddToRoleAsync(
                        studentUser, "Student");

                if (!roleResult.Succeeded)
                {
                    string errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(
                        $"Failed to assign Student role to {username}: {errors}");
                }
            }
        }
    }
}
