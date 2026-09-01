using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;

namespace ResearchProjectManager.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly CourseService _courseService;
        private readonly UserManager<User> _userManager;

        public CourseController(UserManager<User> userManager, CourseService courseService)
        {
            _userManager = userManager;
            _courseService = courseService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = new ViewModels.CoursesViewModel();

            if (User.IsInRole("Student") || User.IsInRole("Instructor"))
            {
                var allCourses = await _courseService.GetAllCoursesAsync();
                viewModel.AllCourses = allCourses
            .Where(c => c.OwnerId == user.Id
                || c.Members.Any(m => m.UserId == user.Id)
                || !c.IsPrivate) 
            .ToList();
            }
            return View(viewModel.AllCourses);
        }
    }
}
