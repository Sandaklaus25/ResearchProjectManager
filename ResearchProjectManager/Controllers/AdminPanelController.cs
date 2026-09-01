using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Services;
using ResearchProjectManager.ViewModels;

namespace ResearchProjectManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelController : Controller
    {
        private readonly AdminPanelService _adminService;

        public AdminPanelController(AdminPanelService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index(string? search, string? role)
        {
            var viewModel = await _adminService.GetFilteredUsersAsync(search, role);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new AdminCreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _adminService.CreateUserAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WipeUser(int id)
        {
            bool success = await _adminService.WipeUserAsync(id);

            if (!success) TempData["Error"] = "Failed to wipe user. Admins cannot be deleted.";
            else TempData["Success"] = "User data completely wiped.";

            return RedirectToAction(nameof(Index));
        }
    }
}