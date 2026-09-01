using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;
using ResearchProjectManager.Repositories.IRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResearchProjectManager.Repositories
{
    public class ProjectTaskRepository : Repository<ProjectTask>, IProjectTaskRepository
    {
        public ProjectTaskRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ProjectTask?> GetTaskWithDetailsByIdAsync(int id) =>
            await _context.ProjectTasks
                .Include(t => t.Author)
                .Include(t => t.Course)
                .Include(t => t.BaseSubtasks)
                .Include(t => t.TaskAssignments)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<List<ProjectTask>> GetTasksByCourseIdWithAuthorAsync(int courseId) =>
            await _context.ProjectTasks
                .Where(pt => pt.CourseId == courseId)
                .Include(pt => pt.Author)
                .ToListAsync();

        public async Task<ProjectTask?> GetTaskWithSubtasksAndAssignmentsAsync(int id) =>
            await _context.ProjectTasks
                .Include(t => t.TaskAssignments)
                .Include(t => t.BaseSubtasks)
                .FirstOrDefaultAsync(t => t.Id == id);

        public void RemoveSubtasks(List<Subtask> subtasks) =>
            _context.Subtasks.RemoveRange(subtasks);
    }
}