using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResearchProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResearchProjectManager.Data
{
    public class DbSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DbSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                await SeedRolesAsync(services);
                await SeedUsersAsync(services);
                await SeedCoursesAsync(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<DbSeeder>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            foreach (var role in Role.PredefinedRoles.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(role.Key))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = role.Key,
                        Description = role.Value
                    });
                }
            }
        }

        private async Task SeedUsersAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            string[] roleNames = { "Admin", "Instructor", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    throw new InvalidOperationException($"FATAL ERROR: Required role '{roleName}' does not exist in the database!");
                }
            }

            var usersToSeed = new[]
            {
                new { Username = "admin", Email = "admin@abv.bg", Role = "Admin" },
                new { Username = "admin123", Email = "admin123@abv.bg", Role = "Admin" },
                new { Username = "teacher", Email = "teacher@abv.bg", Role = "Instructor" },
                new { Username = "teacher123", Email = "teacher123@abv.bg", Role = "Instructor" },
                new { Username = "student", Email = "student@abv.bg", Role = "Student" },
                new { Username = "student123", Email = "student123@abv.bg", Role = "Student" }
            };

            foreach (var userInfo in usersToSeed)
            {
                var existingUser = await userManager.FindByNameAsync(userInfo.Username);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = userInfo.Username,
                        Email = userInfo.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, userInfo.Role);
                    }
                }
            }
        }

        private async Task SeedCoursesAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            if (context.Courses.Any())
            {
                return;
            }

            var teacherUser = await userManager.FindByNameAsync("teacher");
            var teacher123User = await userManager.FindByNameAsync("teacher123");

            if (teacherUser == null || teacher123User == null)
            {
                return;
            }

            // Assume student IDs are 5 and 6 based on your seeder order (admin=1, admin123=2, teacher=3, teacher123=4, student=5, student123=6)
            // Or fetch them dynamically to be completely safe:
            var students = await userManager.GetUsersInRoleAsync("Student");
            var studentIds = students.Select(s => s.Id).ToList();

            // Helper to create members: Owner + All Students (no other teachers)
            List<int> GetCourseMembers(int ownerId)
            {
                var list = new List<int>(studentIds);
                if (!list.Contains(ownerId)) list.Add(ownerId);
                return list;
            }

            var courses = new List<Course>
    {
        // --- TEACHER 1 COURSES ---
        new Course
        {
            Name = "Advanced Software Architecture",
            IsPrivate = true,
            Color = "#D9534F",
            OwnerId = teacherUser.Id,
            Members = GetCourseMembers(teacherUser.Id).Select(id => new CourseMembers { UserId = id }).ToList()
        },
        new Course
        {
            Name = "Database Internals & Tuning",
            IsPrivate = true,
            Color = "#F0AD4E",
            OwnerId = teacherUser.Id,
            Members = GetCourseMembers(teacherUser.Id).Select(id => new CourseMembers { UserId = id }).ToList()
        },
        new Course
        {
            Name = "Introduction to Web Design",
            IsPrivate = false,
            Color = "#8E44AD",
            OwnerId = teacherUser.Id,
            Members = GetCourseMembers(teacherUser.Id).Select(id => new CourseMembers { UserId = id }).ToList()
        },

        // --- TEACHER 2 COURSES ---
        new Course
        {
            Name = "Distributed Systems",
            IsPrivate = true,
            Color = "#f7ea59",
            OwnerId = teacher123User.Id,
            Members = GetCourseMembers(teacher123User.Id).Select(id => new CourseMembers { UserId = id }).ToList()
        },
        new Course
        {
            Name = "Cloud Infrastructure Security",
            IsPrivate = true,
            Color = "#3498DB",
            OwnerId = teacher123User.Id,
            Members = GetCourseMembers(teacher123User.Id).Select(id => new CourseMembers { UserId = id }).ToList()
        },
        new Course
        {
            Name = "Modern UI/UX Principles",
            IsPrivate = false,
            Color = "#16A085",
            OwnerId = teacher123User.Id,
            Members = GetCourseMembers(teacher123User.Id).Select(id => new CourseMembers { UserId = id }).ToList()
        }
    };

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }
    }
}