using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;
using ResearchProjectManager.ViewModels;

namespace ResearchProjectManager.Controllers
{
    public class ProjectTaskController : Controller
    {
        private readonly ProjectTaskService _taskService;

        public ProjectTaskController(ProjectTaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return View(tasks);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Create()
        {
            return View(new ProjectTaskCreateViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectTaskCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newTask = new ProjectTask
                {
                    Name = model.Name,
                    Description = model.Description
                };

                await _taskService.CreateTaskAsync(newTask);

                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }
    }
}