using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;
using ResearchProjectManager.ViewModels;

[Route("Course/{courseId}/ProjectTask/[action]")]
[Authorize(Roles = "Instructor,TA")]
public class ProjectTaskController : Controller
{
    private readonly ProjectTaskService _projectTaskService;
    private readonly CourseService _courseService;
    private readonly UserManager<User> _userManager;

    public ProjectTaskController(ProjectTaskService projectTaskService, CourseService courseService, UserManager<User> userManager)
    {
        _projectTaskService = projectTaskService;
        _courseService = courseService;
        _userManager = userManager;
    }

    private async Task<bool> ValidateCourseAccessAsync(int courseId)
    {
        var userIdString = _userManager.GetUserId(User);
        if (!int.TryParse(userIdString, out int userId)) return false;
        return await _courseService.UserHasAccessAsync(userId, courseId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(int courseId)
    {
        if (!await ValidateCourseAccessAsync(courseId))
        {
            return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
        }

        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null) return NotFound();

        var tasks = await _projectTaskService.GetTasksByCourseIdAsync(courseId);

        var viewModel = new ProjectTaskListViewModel
        {
            Tasks = tasks,
            CourseId = courseId,
            ThemeColor = CourseTheme.Colors[course.Color]
        };

        return View(viewModel);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int courseId, int id)
    {
        if (!await ValidateCourseAccessAsync(courseId))
        {
            return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
        }

        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null) return NotFound();

        var task = await _projectTaskService.GetTaskByIdAsync(id);
        if (task == null || task.CourseId != courseId) return NotFound();

        var viewModel = new ProjectTaskDetailsViewModel
        {
            Task = task,
            CourseId = courseId,
            ThemeColor = CourseTheme.Colors[course.Color]
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int courseId)
    {
        if (!await ValidateCourseAccessAsync(courseId))
        {
            return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
        }

        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null) return NotFound();

        var viewModel = new ProjectTaskCreateViewModel
        {
            CourseId = courseId,
            ThemeColor = CourseTheme.Colors[course.Color]
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, ProjectTaskCreateViewModel model)
    {
        if (!await ValidateCourseAccessAsync(courseId))
        {
            return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
        }

        if (ModelState.IsValid)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _projectTaskService.CreateTaskFromViewModelAsync(model, courseId, userId);
            return RedirectToAction("Index", new { courseId });
        }

        var course = await _courseService.GetCourseByIdAsync(courseId);
        model.CourseId = courseId;
        model.ThemeColor = CourseTheme.Colors[course.Color];
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int courseId, int id)
    {
        if (!await ValidateCourseAccessAsync(courseId))
        {
            return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
        }

        await _projectTaskService.DeleteTaskAsync(id);
        return RedirectToAction(nameof(Index), new { courseId });
    }
}