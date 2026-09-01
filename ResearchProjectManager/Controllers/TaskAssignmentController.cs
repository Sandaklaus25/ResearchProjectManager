using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Helper;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;
using ResearchProjectManager.ViewModels;
using System;
using System.Threading.Tasks;

namespace ResearchProjectManager.Controllers
{
    [Authorize(Roles = "Instructor,TA,Student")]
    public class TaskAssignmentController : Controller
    {
        private readonly TaskAssignmentService _taskAssignmentService;
        private readonly ProjectTaskService _projectTaskService;
        private readonly CourseService _courseService;
        private readonly UserService _userService;

        private readonly TeamService _teamService;

        private readonly UserManager<User> _userManager;

        public TaskAssignmentController(TaskAssignmentService assignmentService, ProjectTaskService projectTaskService, UserService userService, 
            CourseService courseService, TeamService teamService, UserManager<User> userManager)
        {
            _taskAssignmentService = assignmentService;
            _projectTaskService = projectTaskService;
            _userService = userService;
            _courseService = courseService;
            _userManager = userManager;
            _teamService = teamService;
        }
        private async Task<bool> ValidateCourseAccessAsync(int courseId)
        {
            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId)) return false;
            return await _courseService.UserHasAccessAsync(userId, courseId);
        }

        [HttpGet]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> Index(int courseId)
        {
            if (!await ValidateCourseAccessAsync(courseId))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            var course = await _courseService.GetCourseByIdAsync(courseId);
            var userIdString = _userManager.GetUserId(User);

            CookieHelper.SetActiveCourseCookie(Response, HttpContext.Items, userIdString, courseId);

            var assignments = await _taskAssignmentService.GetTaskAssignmentsByCourseIdAsync(courseId);

            var viewModel = new TaskAssignmentListViewModel
            {
                Assignments = assignments,
                CourseId = courseId,
                ThemeColor = CourseTheme.Colors[course.Color]
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> Create(int courseId, int? projectTaskId)
        {
            if (!await ValidateCourseAccessAsync(courseId))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            var course = await _courseService.GetCourseByIdAsync(courseId);

            var viewModel = new AssignTaskViewModel
            {
                CourseId = courseId,
                ThemeColor = CourseTheme.Colors[course.Color],
                ProjectTaskId = projectTaskId ?? 0,
                Deadline = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0).AddDays(7),
                AvailableTasks = await _projectTaskService.GetTasksByCourseIdAsync(courseId),
                AvailableStudents = await _courseService.GetStudentsInCourseAsync(courseId),
                AvailableTeams = await _teamService.GetTeamsByCourseIdAsync(courseId)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> Create(AssignTaskViewModel model)
        {
            if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
            {
                ModelState.AddModelError("Deadline", "The deadline cannot be set in the past.");
            }

            if (ModelState.IsValid)
            {
                if (model.AssignmentMethod == "NewTeam")
                {
                    if (!model.SelectedStudentIds.Any())
                    {
                        ModelState.AddModelError("SelectedStudentIds", "You must select at least one student.");
                    }
                    else
                    {
                        await _taskAssignmentService.AssignToNewGroupAsync(
                            model.ProjectTaskId,
                            model.CourseId,
                            model.SelectedStudentIds,
                            model.Deadline!.Value,
                            model.NewTeamName
                        );
                        return RedirectToAction("Index", new { courseId = model.CourseId });
                    }
                }
                else if (model.AssignmentMethod == "ExistingTeam" && model.SelectedTeamId.HasValue)
                {
                    await _taskAssignmentService.CreateAssignmentAsync(
                        model.ProjectTaskId,
                        model.SelectedTeamId.Value,
                        model.Deadline!.Value
                    );
                    return RedirectToAction("Index", new { courseId = model.CourseId });
                }
            }

            // FORM FAILED REFILL
            var course = await _courseService.GetCourseByIdAsync(model.CourseId);

            model.ThemeColor = course != null && CourseTheme.Colors.ContainsKey(course.Color) ? CourseTheme.Colors[course.Color] : "#0d6efd";

            model.AvailableTasks = await _projectTaskService.GetTasksByCourseIdAsync(model.CourseId);
            model.AvailableStudents = await _courseService.GetStudentsInCourseAsync(model.CourseId);
            model.AvailableTeams = await _teamService.GetTeamsByCourseIdAsync(model.CourseId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            await _taskAssignmentService.DeleteAssignmentAsync(id);
            return RedirectToAction(nameof(Index), new { courseId = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int courseId)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);

            if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(id, userId))
            {
                return Forbid();
            }

            var course = await _courseService.GetCourseByIdAsync(courseId);
            string themeColor = CourseTheme.Colors[course.Color];

            var assignments = await _taskAssignmentService.GetTaskAssignmentsByCourseIdAsync(courseId);
            var details = assignments.FirstOrDefault(a => a.Id == id);

            if (details == null) return NotFound();

            var viewModel = new TaskAssignmentDetailsViewModel
            {
                Assignment = details,
                CourseId = courseId,
                ThemeColor = themeColor,
                BackAction = User.IsInRole("Student") ? "MyCourseAssignments" : "Index",
                IsStudent = User.IsInRole("Student"),
                IsInstructorOrTA = User.IsInRole("Instructor") || User.IsInRole("TA"),
                AllStepsFinished = !details.Subtasks.Any() || details.Subtasks
                .All(s => s.Status == Enums.TaskStatus.Completed || s.Status == Enums.TaskStatus.Cancelled)
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> TurnIn(int id, int courseId, List<IFormFile>? turnInFiles)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(id, userId)) return Forbid();

            await _taskAssignmentService.TurnInAssignmentWithFilesAsync(id, turnInFiles);
            return RedirectToAction(nameof(Details), new { id, courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int assignmentId, int courseId, string commentText, List<IFormFile>? attachmentFiles)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);

            if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(assignmentId, userId))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(commentText) && (attachmentFiles == null || !attachmentFiles.Any()))
            {
                return RedirectToAction(nameof(Details), new { id = assignmentId, courseId });
            }

            await _taskAssignmentService.AddCommentWithFilesAsync(assignmentId, userId, commentText, attachmentFiles);

            return RedirectToAction(nameof(Details), new { id = assignmentId, courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Unsubmit(int id, int courseId)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);

            if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(id, userId))
            {
                return Forbid(); 
            }
            await _taskAssignmentService.UnsubmitAssignmentAsync(id);
            return RedirectToAction(nameof(Details), new { id = id, courseId = courseId });
        }


        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourseAssignments(int courseId)
        {
            if (!await ValidateCourseAccessAsync(courseId))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            var course = await _courseService.GetCourseByIdAsync(courseId);
            
            var userIdString = _userManager.GetUserId(User);

            CookieHelper.SetActiveCourseCookie(Response, HttpContext.Items, userIdString, courseId);

            var userId = int.Parse(userIdString!);
            var assignments = await _taskAssignmentService.GetMyCourseAssignmentsAsync(userId, courseId);

            var viewModel = new TaskAssignmentListViewModel
            {
                Assignments = assignments,
                CourseId = courseId,
                ThemeColor = CourseTheme.Colors[course.Color]
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AllMyAssignments()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var assignments = await _taskAssignmentService.GetMyAssignmentsAsync(userId);

            return View(assignments);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ManageSubtasks(int id, int courseId)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);

            if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(id, userId))
            {
                return Forbid();
            }
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null) throw new Exception($"Course with ID {courseId} not found.");

            var assignment = await _taskAssignmentService.GetTaskAssignmentForSubtaskBoardAsync(id);
            if (assignment == null) return NotFound();

            var viewModel = new SubtaskBoardViewModel
            {
                Assignment = assignment,
                CourseId = courseId,
                ThemeColor = CourseTheme.Colors[course.Color],
                IsStudent = User.IsInRole("Student")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AddSubtask(int assignmentId, int courseId, string name, string? description)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);

                if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(assignmentId, userId))
                {
                    return Forbid();
                }
                await _taskAssignmentService.AddSubtaskToAssignmentAsync(assignmentId, userId, name, description);
            }
            return RedirectToAction(nameof(ManageSubtasks), new { id = assignmentId, courseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateSubtaskStatus(int subtaskId, int assignmentId, int courseId, Enums.TaskStatus status)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);

            if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(assignmentId, userId))
            {
                return Forbid();
            }
            await _taskAssignmentService.UpdateSubtaskStatusAsync(subtaskId, userId, status);
            return RedirectToAction(nameof(ManageSubtasks), new { id = assignmentId, courseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteSubtask(int subtaskId, int assignmentId, int courseId)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);

            if (!await _taskAssignmentService.IsStudentInAssignmentFreePassStaffTeamAsync(assignmentId, userId))
            {
                return Forbid();
            }
            await _taskAssignmentService.DeleteSubtaskAsync(subtaskId);
            return RedirectToAction(nameof(ManageSubtasks), new { id = assignmentId, courseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> GradeAssignment(int id, int courseId, int grade)
        {
            if (!await ValidateCourseAccessAsync(courseId))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }

            await _taskAssignmentService.GradeAssignmentAsync(id, grade);

            return RedirectToAction(nameof(Details), new { id = id, courseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> RemoveGrade(int id, int courseId)
        {
            if (!await ValidateCourseAccessAsync(courseId))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }

            await _taskAssignmentService.RemoveGradeAsync(id);

            return RedirectToAction(nameof(Details), new { id = id, courseId = courseId });
        }
    }
}