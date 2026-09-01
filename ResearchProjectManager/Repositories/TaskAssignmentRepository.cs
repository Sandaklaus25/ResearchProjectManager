using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResearchProjectManager.Repositories
{
    public class TaskAssignmentRepository : Repository<TaskAssignment>, ITaskAssignmentRepository
    {
        public TaskAssignmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<TaskAssignment>> GetAssignmentsForTeamAsync(int teamId) =>
            await _context.TaskAssignments
                .Include(ta => ta.Team)
                .Where(ta => ta.TeamId == teamId)
                .OrderBy(ta => ta.Deadline)
                .ToListAsync();

        public async Task<List<TaskAssignment>> GetAllAssignmentsWithDetailsAsync() =>
            await _context.TaskAssignments
                .Include(ta => ta.Team)
                .Include(ta => ta.Task)
                .OrderBy(ta => ta.Deadline)
                .ToListAsync();

        public async Task<List<TaskAssignment>> GetAssignmentsByCourseIdWithDetailsAsync(int courseId) =>
            await _context.TaskAssignments
                .Include(ta => ta.Task).ThenInclude(t => t.Course)
                .Include(ta => ta.Team).ThenInclude(t => t.UserTeams).ThenInclude(ut => ut.User)
                .Include(ta => ta.Subtasks)
                .Include(ta => ta.TurnInAttachments)
                .Include(ta => ta.Comments).ThenInclude(c => c.User)
                .Include(ta => ta.Comments).ThenInclude(c => c.Attachments)
                .Where(ta => ta.Task.CourseId == courseId)
                .OrderBy(ta => ta.Deadline)
                .AsSplitQuery()
                .ToListAsync();

        public async Task<TaskAssignment?> GetAssignmentWithSubtasksAndTurnInsAsync(int assignmentId) =>
            await _context.TaskAssignments
                .Include(ta => ta.Subtasks)
                .Include(ta => ta.TurnInAttachments)
                .FirstOrDefaultAsync(ta => ta.Id == assignmentId);

        public async Task<TaskAssignment?> GetAssignmentWithTurnInsAsync(int assignmentId) =>
            await _context.TaskAssignments
                .Include(ta => ta.TurnInAttachments)
                .FirstOrDefaultAsync(ta => ta.Id == assignmentId);

        public async Task<List<TaskAssignment>> GetAssignmentsForStudentAsync(int userId) =>
            await _context.TaskAssignments
                .Include(ta => ta.Task).ThenInclude(t => t.Course)
                .Include(ta => ta.Team).ThenInclude(t => t.UserTeams)
                .Include(ta => ta.Subtasks)
                .Where(ta => ta.Team.UserTeams.Any(ut => ut.UserId == userId))
                .OrderBy(ta => ta.Deadline)
                .ToListAsync();

        public async Task<List<TaskAssignment>> GetCourseAssignmentsForStudentAsync(int userId, int courseId) =>
            await _context.TaskAssignments
                .Include(ta => ta.Task).ThenInclude(t => t.Course)
                .Include(ta => ta.Team).ThenInclude(t => t.UserTeams)
                .Include(ta => ta.Subtasks)
                .Where(ta => ta.Task.CourseId == courseId && ta.Team.UserTeams.Any(ut => ut.UserId == userId))
                .OrderBy(ta => ta.Deadline)
                .ToListAsync();

        public async Task<TaskAssignment?> GetAssignmentForSubtaskBoardAsync(int assignmentId) =>
            await _context.TaskAssignments
                .Include(ta => ta.Task).ThenInclude(t => t.Author)
                .Include(ta => ta.Subtasks).ThenInclude(s => s.Creator)
                .Include(ta => ta.Subtasks).ThenInclude(s => s.LastStatusUpdater)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

        public async Task AddTeamAsync(Team team) => await _context.Teams.AddAsync(team);

        public async Task AddUserTeamAsync(UserTeam userTeam) => await _context.UserTeams.AddAsync(userTeam);

        public async Task AddCommentAsync(Comment comment) => await _context.Comments.AddAsync(comment);

        public async Task<Subtask?> GetSubtaskByIdAsync(int subtaskId) => await _context.Subtasks.FindAsync(subtaskId);

        public async Task<Subtask?> GetSubtaskWithAssignmentAsync(int subtaskId) =>
            await _context.Subtasks
                .Include(s => s.TaskAssignment)
                .FirstOrDefaultAsync(s => s.Id == subtaskId);

        public void RemoveSubtasks(List<Subtask> subtasks) => _context.Subtasks.RemoveRange(subtasks);

        public void RemoveAttachments(List<Attachment> attachments) => _context.Attachments.RemoveRange(attachments);

        public async Task<bool> IsStudentInTeamAsync(int assignmentId, int userId) =>
            await _context.TaskAssignments
                .Where(ta => ta.Id == assignmentId)
                .AnyAsync(ta => ta.Team.UserTeams.Any(ut => ut.UserId == userId));
    }
}