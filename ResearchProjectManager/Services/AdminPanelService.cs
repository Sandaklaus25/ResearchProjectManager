using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;
using ResearchProjectManager.ViewModels;
using ResearchProjectManager.ViewModels.Dto;

namespace ResearchProjectManager.Services
{
    public class AdminPanelService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminPanelService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminUserListViewModel> GetFilteredUsersAsync(string? searchTerm, string? roleFilter)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(u => u.UserName.ToLower().Contains(lowerSearch) || u.Email.ToLower().Contains(lowerSearch));
            }

            var users = await query.ToListAsync();
            var userDtos = new List<AdminPanelCreateUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "None";

                if (string.IsNullOrWhiteSpace(roleFilter) || primaryRole.Equals(roleFilter, StringComparison.OrdinalIgnoreCase))
                {
                    userDtos.Add(new AdminPanelCreateUserDto
                    {
                        Id = user.Id,
                        Username = user.UserName,
                        Email = user.Email,
                        Role = primaryRole
                    });
                }
            }

            return new AdminUserListViewModel
            {
                Users = userDtos.OrderBy(u => u.Role).ThenBy(u => u.Username).ToList(),
                SearchTerm = searchTerm,
                RoleFilter = roleFilter
            };
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(AdminCreateUserViewModel model)
        {
            var user = new User { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return (true, "User created successfully.");
            }

            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task<bool> WipeUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            // Admin Immunity Check
            if (await _userManager.IsInRoleAsync(user, "Admin")) return false;

            // nullify ALL shared work globally FIRST. 
            var commentsToAnonymize = await _context.Comments.Where(c => c.UserId == userId).ToListAsync();
            foreach (var c in commentsToAnonymize) c.UserId = null;

            var subtasksCreated = await _context.Subtasks.Where(s => s.CreatorId == userId).ToListAsync();
            foreach (var s in subtasksCreated) s.CreatorId = null;

            var subtasksUpdated = await _context.Subtasks.Where(s => s.LastStatusUpdaterId == userId).ToListAsync();
            foreach (var s in subtasksUpdated) s.LastStatusUpdaterId = null;

            var subtasksAssigned = await _context.Subtasks.Where(s => s.UserId == userId).ToListAsync();
            foreach (var s in subtasksAssigned) s.UserId = null;

            var authoredTasks = await _context.ProjectTasks.Where(pt => pt.AuthorId == userId).ToListAsync();
            foreach (var pt in authoredTasks) pt.AuthorId = null;

            await _context.SaveChangesAsync();

            // Wipe ALL Memberships, Teams, and Bans for this user across ALL courses
            var memberships = await _context.CourseMembers.Where(cm => cm.UserId == userId).ToListAsync();
            _context.CourseMembers.RemoveRange(memberships);

            var userTeams = await _context.UserTeams.Where(ut => ut.UserId == userId).ToListAsync();
            _context.UserTeams.RemoveRange(userTeams);

            var bans = await _context.CourseBans.Where(cb => cb.UserId == userId).ToListAsync();
            _context.CourseBans.RemoveRange(bans);

            await _context.SaveChangesAsync();

            // Full Explosion: Destroy all owned courses by traversing the entire Entity graph
            var ownedCourses = await _context.Courses
                .Include(c => c.Members)
                .Include(c => c.Tasks)
                .Include(c => c.TaskAssignments).ThenInclude(ta => ta.Subtasks)
                .Include(c => c.TaskAssignments).ThenInclude(ta => ta.TurnInAttachments)
                .Include(c => c.TaskAssignments).ThenInclude(ta => ta.Comments).ThenInclude(c => c.Attachments)
                .Where(c => c.OwnerId == userId)
                .ToListAsync();

            if (ownedCourses.Any())
            {
                var courseIds = ownedCourses.Select(c => c.Id).ToList();

                var courseBans = await _context.CourseBans.Where(cb => courseIds.Contains(cb.CourseId)).ToListAsync();
                _context.CourseBans.RemoveRange(courseBans);

                var teams = await _context.Teams
                    .Include(t => t.UserTeams)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.Subtasks)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.TurnInAttachments)
                    .Include(t => t.TaskAssignments).ThenInclude(ta => ta.Comments).ThenInclude(c => c.Attachments)
                    .Where(t => courseIds.Contains(t.CourseId))
                    .ToListAsync();

                var allAssignments = new List<TaskAssignment>();
                foreach (var course in ownedCourses)
                {
                    if (course.TaskAssignments != null) allAssignments.AddRange(course.TaskAssignments);
                }
                foreach (var team in teams)
                {
                    if (team.TaskAssignments != null) allAssignments.AddRange(team.TaskAssignments);
                }
                allAssignments = allAssignments.Distinct().ToList();

                foreach (var assignment in allAssignments)
                {
                    if (assignment.Subtasks != null) _context.Subtasks.RemoveRange(assignment.Subtasks);
                    if (assignment.TurnInAttachments != null) _context.Attachments.RemoveRange(assignment.TurnInAttachments);
                    if (assignment.Comments != null)
                    {
                        foreach (var comment in assignment.Comments)
                        {
                            if (comment.Attachments != null) _context.Attachments.RemoveRange(comment.Attachments);
                        }
                        _context.Comments.RemoveRange(assignment.Comments);
                    }
                }

                _context.TaskAssignments.RemoveRange(allAssignments);

                foreach (var team in teams)
                {
                    if (team.UserTeams != null) _context.UserTeams.RemoveRange(team.UserTeams);
                }
                _context.Teams.RemoveRange(teams);

                // Isolate and destroy base subtasks tied directly to ProjectTasks before dropping the tasks (IF OWNER)
                var projectTasks = ownedCourses.Where(c => c.Tasks != null).SelectMany(c => c.Tasks).ToList();
                if (projectTasks.Any())
                {
                    var projectTaskIds = projectTasks.Select(pt => pt.Id).ToList();
                    var baseSubtasks = await _context.Subtasks
                        .Where(s => s.ProjectTaskId != null && projectTaskIds.Contains(s.ProjectTaskId.Value))
                        .ToListAsync();

                    _context.Subtasks.RemoveRange(baseSubtasks);
                    _context.ProjectTasks.RemoveRange(projectTasks);
                }

                foreach (var course in ownedCourses)
                {
                    if (course.Members != null) _context.CourseMembers.RemoveRange(course.Members);
                }

                _context.Courses.RemoveRange(ownedCourses);

                await _context.SaveChangesAsync();
            }

            // Final Wipe of the actual user account
            var deleteResult = await _userManager.DeleteAsync(user);
            return deleteResult.Succeeded;
        }
    }
}