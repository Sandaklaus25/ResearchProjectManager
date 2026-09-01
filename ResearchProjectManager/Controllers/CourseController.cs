using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Helper;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;
using ResearchProjectManager.ViewModels;

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

            if (User.IsInRole("Student") || User.IsInRole("Instructor") || User.IsInRole("TA"))
            {
                var allCourses = await _courseService.GetAllCoursesOfMemberAsync(user.Id);
                viewModel.AllCourses = allCourses
                .Where(c => c.OwnerId == user.Id
                    || c.Members.Any(m => m.UserId == user.Id)
                    || !c.IsPrivate)
                .ToList();
                viewModel.StudentCounts = await _courseService.GetStudentCountsForCoursesAsync(viewModel.AllCourses);
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId)) return Challenge();

            if (!await _courseService.UserHasAccessAsync(userId, id))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            CookieHelper.SetActiveCourseCookie(Response, HttpContext.Items, userIdString, course.Id);

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                StudentCount = await _courseService.GetStudentCountAsync(id),
                CurrentUserId = userId
            };

            return View(viewModel);
        } 

        [HttpGet]
        public async Task<IActionResult> Members(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null) return NotFound();

            var userIdString = _userManager.GetUserId(User);
            int currentUserId = int.Parse(userIdString!);

            CourseMemberListViewModel viewModel = await _courseService.GetCourseMembersViewModelAsync(courseId, currentUserId);
            viewModel.CourseId = courseId;
            viewModel.ThemeColor = CourseTheme.Colors[course.Color];
            viewModel.OwnerId = course.OwnerId;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> FindMore()
        {
            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Challenge();
            }

            var publicCourses = await _courseService.GetPublicCoursesAsync(userId);
            var studentCounts = await _courseService.GetStudentCountsForCoursesAsync(publicCourses);

            var viewModel = new ViewModels.CoursesViewModel
            {
                AllCourses = publicCourses,
                StudentCounts = studentCounts
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinByCourseCode(string joinCode)
        {
            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Challenge();
            }

            var result = await _courseService.JoinCourseAsync(joinCode, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(FindMore));
            }

            return RedirectToAction(nameof(Details), new { id = result.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId)) return Challenge();

            // Prevent the owner from leaving their own course
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null || course.OwnerId == userId)
            {
                return BadRequest("Course owners cannot leave their own course. Delete the course instead.");
            }

            bool success = await _courseService.LeaveCourseAsync(id, userId);

            if (success)
            {
                Response.Cookies.Delete($"ActiveCourseId_{userId}");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Course { Color = CourseColor.Red});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId)) return Challenge();

            course.OwnerId = userId;

            ModelState.Remove(nameof(Course.SpecialCode));
            ModelState.Remove(nameof(Course.Owner));
            ModelState.Remove(nameof(Course.Tasks));
            ModelState.Remove(nameof(Course.TaskAssignments));
            ModelState.Remove(nameof(Course.Members));

            if (ModelState.IsValid)
            {
                await _courseService.CreateCourseAsync(course);
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId) || course.OwnerId != userId)
            {
                return Challenge();
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id) return NotFound();

            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId) || course.OwnerId != userId)
            {
                return Challenge(); 
            }

            ModelState.Remove(nameof(Course.SpecialCode));
            ModelState.Remove(nameof(Course.Owner));
            ModelState.Remove(nameof(Course.Tasks));
            ModelState.Remove(nameof(Course.TaskAssignments));
            ModelState.Remove(nameof(Course.Members));

            if (ModelState.IsValid)
            {
                var updatedCourse = await _courseService.UpdateCourseAsync(id, course);
                if (updatedCourse == null) return NotFound();

                return RedirectToAction(nameof(Details), new { id = course.Id });
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId)) return Challenge();

            // Verify ownership before deleting
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null || course.OwnerId != userId)
            {
                return Challenge();
            }

            await _courseService.DeleteCourseAsync(id);

            Response.Cookies.Delete($"ActiveCourseId_{userId}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> ManageMemberStatus(int courseId, int targetUserId, string actionType)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
            if (targetUser == null || currentUser == null) return NotFound();

            bool isOwner = course.OwnerId == currentUser.Id;
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            bool canManage = false;

            // HIERARCHY LOGIC Owner> Instructor > TA > Student
            if (isOwner)
            {
                canManage = targetUserId != currentUser.Id; 
            }
            else if (currentUserRoles.Contains("Instructor"))
            {
                canManage = !targetUserRoles.Contains("Instructor") && course.OwnerId != targetUserId;
            }
            else if (currentUserRoles.Contains("TA"))
            {
                canManage = targetUserRoles.Contains("Student") && !targetUserRoles.Contains("Instructor") && !targetUserRoles.Contains("TA") && course.OwnerId != targetUserId;
            }

            if (!canManage) return Challenge();

            if (actionType == "Kick")
            {
                await _courseService.KickMemberAsync(courseId, targetUserId);
            }
            else if (actionType == "Ban")
            {
                await _courseService.BanMemberAsync(courseId, targetUserId);
            }

            return RedirectToAction(nameof(Members), new { courseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,TA")]
        public async Task<IActionResult> UnbanMember(int courseId, int targetUserId)
        {
            // Basic check: Only Instructors and Owners can unban (TAs cannot reverse bans)
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser!);
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course?.OwnerId != currentUser!.Id && !roles.Contains("Instructor"))
            {
                return Forbid();
            }

            await _courseService.UnbanMemberAsync(courseId, targetUserId);
            return RedirectToAction(nameof(Members), new { courseId = courseId });
        }
    }
}