using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Models;
using ResearchProjectManager.Repositories.IRepositories;
using ResearchProjectManager.ViewModels;

namespace ResearchProjectManager.Services
{
    public class CourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly UserManager<User> _userManager;

        public CourseService(ICourseRepository courseRepository, UserManager<User> userManager)
        {
            _courseRepository = courseRepository;
            _userManager = userManager;
        }

        public async Task<List<Course>> GetAllCoursesOfMemberAsync(int userId)
        {
            return await _courseRepository.GetAllCoursesOfMemberWithDetailsAsync(userId);
        }

        public async Task<List<Course>> GetPublicCoursesAsync(int userId)
        {
            return await _courseRepository.GetPublicCoursesWithDetailsAsync(userId);
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _courseRepository.GetCourseWithDetailsByIdAsync(id);
        }

        public async Task<bool> UserHasAccessAsync(int userId, int courseId)
        {
            var course = await _courseRepository.GetCourseWithDetailsByIdAsync(courseId);
            if (course == null) return false;

            return course.OwnerId == userId ||
                   course.Members.Any(m => m.UserId == userId) ||
                   !course.IsPrivate;
        }

        public async Task<(bool Success, string Message, int? CourseId)> JoinCourseAsync(string joinCode, int userId)
        {
            var course = await _courseRepository.GetCourseBySpecialCodeAsync(joinCode);

            if (course == null) return (false, "Invalid course access code.", null);

            bool isBanned = await _courseRepository.IsBannedAsync(course.Id, userId);
            if (isBanned) return (false, "You have been banned from this course.", null);

            bool alreadyMember = course.Members.Any(m => m.UserId == userId) || course.OwnerId == userId;

            if (!alreadyMember)
            {
                await _courseRepository.AddCourseMemberAsync(new CourseMember
                {
                    CourseId = course.Id,
                    UserId = userId
                });
                await _courseRepository.SaveChangesAsync();
            }

            return (true, string.Empty, course.Id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            do
            {
                course.SpecialCode = Helpers.CodeGenerator.GenerateSpecialCode();
            }
            while ((await _courseRepository.FindAsync(c => c.SpecialCode == course.SpecialCode)).Any());

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();
            return course;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return false;

            _courseRepository.Remove(course);
            await _courseRepository.SaveChangesAsync();
            return true;
        }

        public async Task<CourseMemberListViewModel> GetCourseMembersViewModelAsync(int courseId, int currentUserId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
            var currentRoles = await _userManager.GetRolesAsync(currentUser!);

            var viewModel = new CourseMemberListViewModel
            {
                CurrentUserId = currentUserId,
                IsCurrentUserOwner = course?.OwnerId == currentUserId,
                CurrentUserRole = currentRoles.FirstOrDefault() ?? string.Empty
            };

            var courseMembers = await _courseRepository.GetCourseMembersAsync(courseId);

            foreach (var member in courseMembers)
            {
                if (member.User == null) continue;
                var roles = await _userManager.GetRolesAsync(member.User);

                if (roles.Contains("Instructor")) viewModel.Instructors.Add(member.User);
                else if (roles.Contains("TA")) viewModel.Assistants.Add(member.User);
                else if (roles.Contains("Student")) viewModel.Students.Add(member.User);
            }

            var bans = await _courseRepository.GetCourseBansAsync(courseId);
            viewModel.BannedUsers = bans.Select(b => b.User).ToList();

            return viewModel;
        }

        public async Task<bool> IsUserMemberAsync(int courseId, int userId)
        {
            var course = await _courseRepository.GetCourseWithDetailsByIdAsync(courseId);
            if (course == null) return false;

            return course.OwnerId == userId || course.Members.Any(m => m.UserId == userId);
        }

        public async Task<int> GetStudentCountAsync(int courseId)
        {
            var courseMembers = await _courseRepository.GetCourseMembersAsync(courseId);
            int studentCount = 0;

            foreach (var member in courseMembers)
            {
                if (member.User != null && await _userManager.IsInRoleAsync(member.User, "Student"))
                {
                    studentCount++;
                }
            }

            return studentCount;
        }

        public async Task<Dictionary<int, int>> GetStudentCountsForCoursesAsync(List<Course> courses)
        {
            var counts = new Dictionary<int, int>();
            foreach (var course in courses)
            {
                counts[course.Id] = await GetStudentCountAsync(course.Id);
            }
            return counts;
        }

        public async Task<List<User>> GetStudentsInCourseAsync(int courseId)
        {
            var courseMembers = await _courseRepository.GetCourseMembersAsync(courseId);
            var students = new List<User>();

            foreach (var member in courseMembers)
            {
                if (member.User != null && await _userManager.IsInRoleAsync(member.User, "Student"))
                {
                    students.Add(member.User);
                }
            }

            return students;
        }

        public async Task<bool> LeaveCourseAsync(int courseId, int userId)
        {
            var courseMember = await _courseRepository.GetCourseMemberAsync(courseId, userId);
            if (courseMember == null) return false;

            await _courseRepository.RemoveCourseMemberAsync(courseMember);
            await _courseRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Course?> UpdateCourseAsync(int courseId, Course updatedCourse)
        {
            var existingCourse = await _courseRepository.GetByIdAsync(courseId);
            if (existingCourse == null) return null;

            existingCourse.Name = updatedCourse.Name;
            existingCourse.Description = updatedCourse.Description;
            existingCourse.IsPrivate = updatedCourse.IsPrivate;
            existingCourse.Color = updatedCourse.Color;

            _courseRepository.Update(existingCourse);
            await _courseRepository.SaveChangesAsync();
            return existingCourse;
        }

        public async Task<bool> KickMemberAsync(int courseId, int targetUserId)
        {
            var membership = await _courseRepository.GetCourseMemberAsync(courseId, targetUserId);
            if (membership != null)
            {
                await _courseRepository.RemoveCourseMemberAsync(membership);
            }

            var teamMemberships = await _courseRepository.GetUserTeamsForCourseAsync(courseId, targetUserId);
            if (teamMemberships.Any())
            {
                _courseRepository.RemoveUserTeams(teamMemberships);
            }

            await _courseRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BanMemberAsync(int courseId, int targetUserId)
        {
            await KickMemberAsync(courseId, targetUserId);

            bool isAlreadyBanned = await _courseRepository.IsBannedAsync(courseId, targetUserId);
            if (!isAlreadyBanned)
            {
                await _courseRepository.AddCourseBanAsync(new CourseBan
                {
                    CourseId = courseId,
                    UserId = targetUserId,
                    BannedAt = DateTime.Now
                });
                await _courseRepository.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> UnbanMemberAsync(int courseId, int targetUserId)
        {
            var bans = await _courseRepository.GetCourseBansAsync(courseId);
            var ban = bans.FirstOrDefault(cb => cb.UserId == targetUserId);

            if (ban != null)
            {
                await _courseRepository.RemoveCourseBanAsync(ban);
                await _courseRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}