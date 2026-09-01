using ResearchProjectManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchProjectManager.Repositories.IRepositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<List<Course>> GetAllCoursesOfMemberWithDetailsAsync(int userId);
        Task<List<Course>> GetPublicCoursesWithDetailsAsync(int userId);
        Task<Course?> GetCourseWithDetailsByIdAsync(int id);
        Task<Course?> GetCourseBySpecialCodeAsync(string specialCode);
        Task<List<CourseMember>> GetCourseMembersAsync(int courseId);
        Task<List<CourseBan>> GetCourseBansAsync(int courseId);
        Task AddCourseMemberAsync(CourseMember member);
        Task RemoveCourseMemberAsync(CourseMember member);
        Task AddCourseBanAsync(CourseBan ban);
        Task RemoveCourseBanAsync(CourseBan ban);
        Task<bool> IsBannedAsync(int courseId, int userId);
        Task<CourseMember?> GetCourseMemberAsync(int courseId, int userId);
        Task<List<UserTeam>> GetUserTeamsForCourseAsync(int courseId, int targetUserId);
        void RemoveUserTeams(List<UserTeam> userTeams);
    }
}