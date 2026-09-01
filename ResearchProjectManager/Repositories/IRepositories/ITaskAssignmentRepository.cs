using ResearchProjectManager.Models;
using ResearchProjectManager.Repositories.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchProjectManager.Repositories
{
    public interface ITaskAssignmentRepository : IRepository<TaskAssignment>
    {
        Task<List<TaskAssignment>> GetAssignmentsForTeamAsync(int teamId);
        Task<List<TaskAssignment>> GetAllAssignmentsWithDetailsAsync();
        Task<List<TaskAssignment>> GetAssignmentsByCourseIdWithDetailsAsync(int courseId);
        Task<TaskAssignment?> GetAssignmentWithSubtasksAndTurnInsAsync(int assignmentId);
        Task<TaskAssignment?> GetAssignmentWithTurnInsAsync(int assignmentId);
        Task<List<TaskAssignment>> GetAssignmentsForStudentAsync(int userId);
        Task<List<TaskAssignment>> GetCourseAssignmentsForStudentAsync(int userId, int courseId);
        Task<TaskAssignment?> GetAssignmentForSubtaskBoardAsync(int assignmentId);
        Task AddTeamAsync(Team team);
        Task AddUserTeamAsync(UserTeam userTeam);
        Task AddCommentAsync(Comment comment);
        Task<Subtask?> GetSubtaskByIdAsync(int subtaskId);
        Task<Subtask?> GetSubtaskWithAssignmentAsync(int subtaskId);
        void RemoveSubtasks(List<Subtask> subtasks);
        void RemoveAttachments(List<Attachment> attachments);
        Task<bool> IsStudentInTeamAsync(int assignmentId, int userId);
    }
}