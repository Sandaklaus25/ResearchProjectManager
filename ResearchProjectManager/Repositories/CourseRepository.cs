using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;
using ResearchProjectManager.Repositories.IRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResearchProjectManager.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<Course>> GetAllCoursesOfMemberWithDetailsAsync(int userId) =>
            await _context.Courses
                .Include(c => c.Owner)
                .Include(c => c.Members)
                .Where(c => c.Members.Any(m => m.UserId == userId))
                .ToListAsync();

        public async Task<List<Course>> GetPublicCoursesWithDetailsAsync(int userId) =>
            await _context.Courses
                .Include(c => c.Owner)
                .Include(c => c.Members)
                .Where(c => !c.IsPrivate && c.OwnerId != userId && !c.Members.Any(m => m.UserId == userId))
                .ToListAsync();

        public async Task<Course?> GetCourseWithDetailsByIdAsync(int id) =>
            await _context.Courses
                .Include(c => c.Owner)
                .Include(c => c.Members)
                .Include(c => c.Tasks)
                .Include(c => c.TaskAssignments)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Course?> GetCourseBySpecialCodeAsync(string specialCode) =>
            await _context.Courses
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.SpecialCode == specialCode);

        public async Task<List<CourseMember>> GetCourseMembersAsync(int courseId) =>
            await _context.CourseMembers
                .Where(cm => cm.CourseId == courseId)
                .Include(cm => cm.User)
                .ToListAsync();

        public async Task<List<CourseBan>> GetCourseBansAsync(int courseId) =>
            await _context.CourseBans
                .Where(cb => cb.CourseId == courseId)
                .Include(cb => cb.User)
                .ToListAsync();

        public async Task AddCourseMemberAsync(CourseMember member) => await _context.CourseMembers.AddAsync(member);

        public async Task RemoveCourseMemberAsync(CourseMember member) { _context.CourseMembers.Remove(member); await Task.CompletedTask; }

        public async Task AddCourseBanAsync(CourseBan ban) => await _context.CourseBans.AddAsync(ban);

        public async Task RemoveCourseBanAsync(CourseBan ban) { _context.CourseBans.Remove(ban); await Task.CompletedTask; }

        public async Task<bool> IsBannedAsync(int courseId, int userId) =>
            await _context.CourseBans.AnyAsync(cb => cb.CourseId == courseId && cb.UserId == userId);

        public async Task<CourseMember?> GetCourseMemberAsync(int courseId, int userId) =>
            await _context.CourseMembers.FirstOrDefaultAsync(cm => cm.CourseId == courseId && cm.UserId == userId);

        public async Task<List<UserTeam>> GetUserTeamsForCourseAsync(int courseId, int targetUserId) =>
            await _context.UserTeams
                .Include(ut => ut.Team)
                .Where(ut => ut.UserId == targetUserId && ut.Team.CourseId == courseId)
                .ToListAsync();

        public void RemoveUserTeams(List<UserTeam> userTeams) => _context.UserTeams.RemoveRange(userTeams);
    }
}