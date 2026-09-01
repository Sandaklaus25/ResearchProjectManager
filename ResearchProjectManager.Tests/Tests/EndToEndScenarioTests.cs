using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using ResearchProjectManager.Data;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Models;
using ResearchProjectManager.Repositories;
using ResearchProjectManager.Repositories.IRepositories;
using ResearchProjectManager.Services;
using ResearchProjectManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ResearchProjectManager.Tests
{
    public class EndToEndScenarioTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminPanelService _adminPanelService;
        private readonly CourseService _courseService;
        private readonly ProjectTaskService _projectTaskService;
        private readonly TaskAssignmentService _taskAssignmentService;
        private readonly Mock<UserManager<User>> _mockUserManager;

        public EndToEndScenarioTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Callback<User, string>((u, p) => { _context.Users.Add(u); _context.SaveChanges(); })
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => _context.Users.Find(int.Parse(id)));

            ICourseRepository courseRepo = new CourseRepository(_context);
            _courseService = new CourseService(courseRepo, _mockUserManager.Object);

            IProjectTaskRepository projectTaskRepo = new ProjectTaskRepository(_context);
            _projectTaskService = new ProjectTaskService(projectTaskRepo);

            ITaskAssignmentRepository taskAssignmentRepo = new TaskAssignmentRepository(_context);
            _taskAssignmentService = new TaskAssignmentService(taskAssignmentRepo, projectTaskRepo, _mockUserManager.Object);

            _adminPanelService = new AdminPanelService(_context, _mockUserManager.Object);
        }

        [Fact]
        public async Task Step1_AdminCreatesUsersOfEachRole()
        {
            await _adminPanelService.CreateUserAsync(new AdminCreateUserViewModel { Username = "TeacherProf", Email = "prof@uni.bg", Password = "Password123!", Role = "Instructor" });
            await _adminPanelService.CreateUserAsync(new AdminCreateUserViewModel { Username = "TAGreg", Email = "ta@uni.bg", Password = "Password123!", Role = "TA" });
            await _adminPanelService.CreateUserAsync(new AdminCreateUserViewModel { Username = "StudentAlice", Email = "alice@uni.bg", Password = "Password123!", Role = "Student" });
            await _adminPanelService.CreateUserAsync(new AdminCreateUserViewModel { Username = "StudentBob", Email = "bob@uni.bg", Password = "Password123!", Role = "Student" });

            Assert.Equal(4, _context.Users.Count());
        }

        [Fact]
        public async Task Step2_TeacherCreatesCourse()
        {
            var teacher = new User { Id = 1, UserName = "TeacherProf" };
            _context.Users.Add(teacher);
            await _context.SaveChangesAsync();

            var courseToCreate = new Course
            {
                Name = "Software Engineering",
                Description = "Architecture and Clean Code",
                OwnerId = teacher.Id,
                Color = CourseColor.Blue
            };

            var createdCourse = await _courseService.CreateCourseAsync(courseToCreate);

            Assert.NotNull(createdCourse);
            Assert.False(string.IsNullOrEmpty(createdCourse.SpecialCode));
            Assert.Equal(teacher.Id, createdCourse.OwnerId);
        }

        [Fact]
        public async Task Step3_JoinCourseViaCode()
        {
            var teacher = new User { Id = 1, UserName = "TeacherProf" };
            var ta = new User { Id = 2, UserName = "TAGreg" };
            var alice = new User { Id = 3, UserName = "StudentAlice" };
            _context.Users.AddRange(teacher, ta, alice);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.IsInRoleAsync(alice, "Student")).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.IsInRoleAsync(ta, "Student")).ReturnsAsync(false);

            var course = new Course
            {
                Id = 1,
                Name = "Software Engineering",
                OwnerId = teacher.Id,
                Color = CourseColor.Blue,
                SpecialCode = "TEST-JOIN-CODE"
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var joinAlice = await _courseService.JoinCourseAsync("TEST-JOIN-CODE", alice.Id);
            var joinTa = await _courseService.JoinCourseAsync("TEST-JOIN-CODE", ta.Id);

            Assert.True(joinAlice.Success);
            Assert.True(joinTa.Success);

            var rosterCount = await _courseService.GetStudentCountAsync(course.Id);
            Assert.Equal(1, rosterCount);
        }

        [Fact]
        public async Task Step4_CreatesProjectTaskAndGroupAssignment()
        {
            var teacher = new User { Id = 1, UserName = "TeacherProf" };
            var ta = new User { Id = 2, UserName = "TAGreg" };
            var alice = new User { Id = 3, UserName = "StudentAlice" };
            var bob = new User { Id = 4, UserName = "StudentBob" };
            _context.Users.AddRange(teacher, ta, alice, bob);

            var course = new Course
            {
                Id = 1,
                Name = "Software Engineering",
                OwnerId = teacher.Id,
                Color = CourseColor.Blue,
                SpecialCode = "TEST-CODE"
            };
            _context.Courses.Add(course);

            var projectTask = new ProjectTask
            {
                Id = 1,
                Name = "Design Document",
                Description = "Write system architecture specs",
                CourseId = course.Id,
                AuthorId = ta.Id,
                CreatedAt = DateTime.Now
            };
            _context.ProjectTasks.Add(projectTask);
            await _context.SaveChangesAsync();

            var studentIds = new List<int> { alice.Id, bob.Id };
            await _taskAssignmentService.AssignToNewGroupAsync(
                projectTask.Id,
                course.Id,
                studentIds,
                DateTime.Now.AddDays(5),
                "Team Architecture"
            );

            var team = await _context.Teams.Include(t => t.UserTeams).FirstAsync(t => t.Name == "Team Architecture");
            var assignment = await _context.TaskAssignments.FirstAsync(taItem => taItem.TeamId == team.Id);

            Assert.NotNull(assignment);
            Assert.Equal(2, team.UserTeams.Count);
        }

        [Fact]
        public async Task Step5_StudentsUseChatOnAssignment()
        {
            var teacher = new User { Id = 1, UserName = "TeacherProf" };
            var alice = new User { Id = 3, UserName = "StudentAlice" };
            var bob = new User { Id = 4, UserName = "StudentBob" };
            _context.Users.AddRange(teacher, alice, bob);

            var course = new Course { Id = 1, Name = "SE", OwnerId = teacher.Id, Color = CourseColor.Blue, SpecialCode = "C1" };
            var projectTask = new ProjectTask { Id = 1, Name = "Task 1", Description = "Task description", CourseId = 1 };
            var team = new Team { Id = 1, Name = "Team 1", CourseId = 1 };
            var assignment = new TaskAssignment { Id = 1, Name = "Assignment 1", TaskId = 1, TeamId = 1, Deadline = DateTime.Now.AddDays(1) };

            _context.Courses.Add(course);
            _context.ProjectTasks.Add(projectTask);
            _context.Teams.Add(team);
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            await _taskAssignmentService.AddCommentAsync(assignment.Id, alice.Id, "Hey Bob, I will draft the sequence diagrams.", null);
            await _taskAssignmentService.AddCommentAsync(assignment.Id, bob.Id, "Awesome, I'll review them tonight.", new List<(string, string)>());

            var comments = await _context.Comments.Where(c => c.TaskAssignmentId == assignment.Id).ToListAsync();

            Assert.Equal(2, comments.Count);
            Assert.Contains(comments, c => c.CommentText.Contains("sequence diagrams"));
            Assert.Contains(comments, c => c.CommentText.Contains("review them tonight"));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}