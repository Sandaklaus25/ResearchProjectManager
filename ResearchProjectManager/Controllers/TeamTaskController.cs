using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;
using ResearchProjectManager.ViewModels;
using System;
using System.Threading.Tasks;

namespace ResearchProjectManager.Controllers
{
    // Only Instructors and Admins should be allowed to hand out assignments!
    [Authorize(Roles = "Instructor,Admin")]
    public class TeamTaskController : Controller
    {
        private readonly TaskAssignmentService _assignmentService;
        private readonly ProjectTaskService _projectTaskService;
        private readonly UserService _userService;

        public TeamTaskController(TaskAssignmentService assignmentService, ProjectTaskService projectTaskService, UserService userService)
        {
            _assignmentService = assignmentService;
            _projectTaskService = projectTaskService;
            _userService = userService;
        }

        // ==========================================
        // GET: /TeamTask/Create?projectTaskId=5
        // Shows the roster so the instructor can pick students
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create(int projectTaskId)
        {
            // FIX 1: Use _projectTaskService instead of _taskService
            var task = await _projectTaskService.GetTaskByIdAsync(projectTaskId);

            if (task == null)
            {
                return NotFound();
            }

            // Build the form
            var viewModel = new AssignTaskViewModel
            {
                ProjectTaskId = task.Id,
                TaskName = task.Name,
                AvailableStudents = await _userService.GetAllStudentsAsync() // Grabs the roster!
            };

            return View(viewModel);
        }

        // ==========================================
        // POST: /TeamTask/Create
        // Saves the new group and assignment to the database
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                // FIX 2: Use _assignmentService instead of _taskService
                // FIX 3: Use the new AssignToNewGroupAsync method we created
                await _assignmentService.AssignToNewGroupAsync(
                    model.ProjectTaskId,
                    model.SelectedStudentIds,
                    model.Deadline!.Value // (! - trust me) We know the deadline is required, so we can safely use .Value here
                );

                // Success! Send the instructor back to the Dashboard to see it.
                return RedirectToAction("Index", "Dashboard");
            }

            // If they forgot to select any students and the form failed, 
            // we have to reload the roster so the page doesn't crash.
            model.AvailableStudents = await _userService.GetAllStudentsAsync();
            return View(model);
        }
    }
}