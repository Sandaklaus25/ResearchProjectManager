using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;

namespace ResearchProjectManager.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ProjectTaskService _projectTaskService;
        private readonly TaskAssignmentService _taskAssignmentService;
        private readonly UserManager<User> _userManager;

        public DashboardController(ProjectTaskService projectTaskService, TaskAssignmentService taskAssignmentService, UserManager<User> userManager)
        {
            _projectTaskService = projectTaskService;
            _taskAssignmentService = taskAssignmentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = new ViewModels.DashboardViewModel();
            if (User.IsInRole("Admin") || User.IsInRole("Instructor"))
            {
                // Admins and Instructors see all high-level projects
                viewModel.AllProjects = await _projectTaskService.GetAllTasksAsync();
            }
            else if (User.IsInRole("Student"))
            {
                // Students see their specific active workloads
                viewModel.ActiveAssignments = await _taskAssignmentService.GetAllAssignmentsAsync();
            }
            return View(viewModel);
        }
    }
}
