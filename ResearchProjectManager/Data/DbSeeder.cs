using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Helpers;
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

            string[] roleNames = { "Admin", "Instructor", "TA", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    throw new InvalidOperationException($"FATAL ERROR: Required role '{roleName}' does not exist in the database!");
                }
            }

            var usersToSeed = new[] {
                new { Username = "admin", Email = "admin@abv.bg", Role = "Admin" },
                new { Username = "admin123", Email = "admin123@abv.bg", Role = "Admin" },

                new { Username = "teacher", Email = "teacher@abv.bg", Role = "Instructor" },
                new { Username = "teacher123", Email = "teacher123@abv.bg", Role = "Instructor" },
                new { Username = "teacher45", Email = "teacher45@abv.bg", Role = "Instructor" },
                new { Username = "teacher67", Email = "teacher67@abv.bg", Role = "Instructor" },
    
                new { Username = "ta1", Email = "ta@abv.bg", Role = "TA" },
                new { Username = "ta2", Email = "ta123@abv.bg", Role = "TA" },
                new { Username = "ta3", Email = "ta45@abv.bg", Role = "TA" },

                new { Username = "student", Email = "student@abv.bg", Role = "Student" },
                new { Username = "student123", Email = "student123@abv.bg", Role = "Student" },

                new { Username = "student10", Email = "student10@abv.bg", Role = "Student" },
                new { Username = "student11", Email = "student11@abv.bg", Role = "Student" },
                new { Username = "student12", Email = "student12@abv.bg", Role = "Student" },
                new { Username = "student13", Email = "student13@abv.bg", Role = "Student" },
                new { Username = "student14", Email = "student14@abv.bg", Role = "Student" }
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
            var teacher45User = await userManager.FindByNameAsync("teacher45");
            var teacher67User = await userManager.FindByNameAsync("teacher67");

            if (teacherUser == null || teacher123User == null || teacher45User == null || teacher67User == null)
            {
                return;
            }

            var students = await userManager.GetUsersInRoleAsync("Student");
            var studentIds = students.Select(s => s.Id).ToList();

            List<int> GetCourseMembers(int ownerId)
            {
                var list = new List<int>(studentIds);
                if (!list.Contains(ownerId)) list.Add(ownerId);
                return list;
            }

            var courses = new List<Course>
            {
                // TEACHER 1 COURSES
                new Course
                {
                    Name = "Advanced Software Architecture",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = true,
                    Color = CourseColor.Red,
                    OwnerId = teacherUser.Id,
                    Members = GetCourseMembers(teacherUser.Id).Select(id => new CourseMember { UserId = id }).ToList()
                },
                new Course
                {
                    Name = "Database Internals & Tuning",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = true,
                    Color = CourseColor.Orange,
                    OwnerId = teacherUser.Id,
                    Members = GetCourseMembers(teacherUser.Id).Select(id => new CourseMember { UserId = id }).ToList()
                },
                new Course
                {
                    Name = "Introduction to Web Design",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = false,
                    Color = CourseColor.Purple,
                    OwnerId = teacherUser.Id,
                    Members = GetCourseMembers(teacherUser.Id).Select(id => new CourseMember { UserId = id }).ToList()
                },

                // TEACHER 2 COURSES 
                new Course
                {
                    Name = "Distributed Systems",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = true,
                    Color = CourseColor.Brown,
                    OwnerId = teacher123User.Id,
                    Members = GetCourseMembers(teacher123User.Id).Select(id => new CourseMember { UserId = id }).ToList()
                },
                new Course
                {
                    Name = "Cloud Infrastructure Security",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = true,
                    Color = CourseColor.Blue,
                    OwnerId = teacher123User.Id,
                    Members = GetCourseMembers(teacher123User.Id).Select(id => new CourseMember { UserId = id }).ToList()
                },
                new Course
                {
                    Name = "Modern UI/UX Principles",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = false,
                    Color = CourseColor.Teal,
                    OwnerId = teacher123User.Id,
                    Members = GetCourseMembers(teacher123User.Id).Select(id => new CourseMember { UserId = id }).ToList()
                },

                // TEACHER 3 COURSES 
                new Course
                {
                    Name = "Data Structures & Algorithms",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = false,
                    Color = CourseColor.Red,
                    OwnerId = teacher45User.Id,
                    Members = new List<CourseMember> { new CourseMember { UserId = teacher45User.Id } }
                },
                new Course
                {
                    Name = "Artificial Intelligence Foundations",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = false,
                    Color = CourseColor.Orange,
                    OwnerId = teacher45User.Id,
                    Members = new List<CourseMember> { new CourseMember { UserId = teacher45User.Id } }
                },

                // TEACHER 4 COURSES
                new Course
                {
                    Name = "Mobile Application Development",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = false,
                    Color = CourseColor.Purple,
                    OwnerId = teacher67User.Id,
                    Members = new List<CourseMember> { new CourseMember { UserId = teacher67User.Id } }
                },
                new Course
                {
                    Name = "DevOps & Continuous Integration",
                    SpecialCode = CodeGenerator.GenerateSpecialCode(),
                    IsPrivate = false,
                    Color = CourseColor.Blue,
                    OwnerId = teacher67User.Id,
                    Members = new List<CourseMember> { new CourseMember { UserId = teacher67User.Id } }
                }
            };

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }
    }
}