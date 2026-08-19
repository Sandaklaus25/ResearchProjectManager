using ResearchProjectManager.Models;
using ResearchProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Enums;

namespace ResearchProjectManager.Services
{
    public class TaskAssignmentService
    {
        private readonly ApplicationDbContext _context;
        public TaskAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskAssignment>> GetTaskAssignmentsForTeamAsync(int teamId)
        {
            return await _context.TaskAssignments
                .Include(ta => ta.Team)
                .Where(ta => ta.TeamId == teamId)
                .OrderBy(ta => ta.Deadline)
                .ToListAsync();
        }
        //Test feature only, remove later
        public async Task<List<TaskAssignment>> GetAllAssignmentsAsync()
        {
            return await _context.TaskAssignments
                .Include(ta => ta.Team)
                .Include(ta => ta.Task)
                .OrderBy(ta => ta.Deadline)
                .ToListAsync();
        }

        public async Task<TaskAssignment?> GetTaskAssignmentDetailsByIdAsync(int id)
        {
            return await _context.TaskAssignments
                .Include(ta => ta.Task)              // The main task instructions
                .Include(ta => ta.Team)              // The team assigned
                    .ThenInclude(t => t.UserTeams)   // The specific users in that team
                        .ThenInclude(ut => ut.User)
                .Include(ta => ta.Subtasks)          // The checklist
                .Include(ta => ta.Comments)          // The conversation history
                    .ThenInclude(c => c.User)        // Who made the comments
                .Include(ta => ta.Attachments)       // The files uploaded
                    .ThenInclude(a => a.User)        // Who uploaded the files
                .FirstOrDefaultAsync(ta => ta.Id == id);
        }

        public async Task UpdateAssignmentStatusAsync(int assignmentId, TaskStatusEnum newStatus)
        {
            var assignment = await _context.TaskAssignments.FindAsync(assignmentId);
            if (assignment != null)
            {
                assignment.TaskStatus = newStatus;
                if (newStatus == TaskStatusEnum.Completed) assignment.CompletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateDeadLineAsync(int assignmentId, DateTime newDeadline)
        {
            var assignment = await _context.TaskAssignments.FindAsync(assignmentId);
            if (assignment != null)
            {
                assignment.Deadline = newDeadline;
                await _context.SaveChangesAsync();
            }
        }

        // ==========================================
        // Takes the checked students, forms a group, and assigns the task
        // ==========================================
        public async Task AssignToNewGroupAsync(int projectTaskId, List<int> selectedStudentIds, DateTime deadline)
        {
            // 1. Create a brand new Team for this specific assignment
            var newTeam = new Team
            {
                Name = $"Group Project - Task #{projectTaskId}"
            };

            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync(); // Save to generate the new Team.Id

            // 2. Loop through every checked student and add them to the new Team
            foreach (var studentId in selectedStudentIds)
            {
                var userTeam = new UserTeam
                {
                    UserId = studentId,
                    TeamId = newTeam.Id
                };
                _context.UserTeams.Add(userTeam);
            }

            // 3. Create the actual Assignment connecting the Prompt, the Team, and the Deadline
            var assignment = new TaskAssignment
            {
                TaskId = projectTaskId,
                TeamId = newTeam.Id,
                Deadline = deadline,
                TaskStatus = TaskStatusEnum.NotStarted,
                CompletedAt = null
            };

            _context.TaskAssignments.Add(assignment);

            // 4. Save everything to the database!
            await _context.SaveChangesAsync();
        }
        public async Task<TaskAssignment> CreateAssignmentAsync(int projectTaskId, int teamId, DateTime deadline)
        {
            var assignment = new TaskAssignment
            {
                TaskId = projectTaskId,
                TeamId = teamId,
                Deadline = deadline,
                TaskStatus = TaskStatusEnum.NotStarted,
                CompletedAt = null
            };

            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return assignment;
        }
    }
}
