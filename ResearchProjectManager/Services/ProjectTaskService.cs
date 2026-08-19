using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.Services
{
    public class ProjectTaskService
    {
        private readonly ApplicationDbContext _context;

        public ProjectTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectTask>> GetAllTasksAsync()
        {
            return await _context.ProjectTasks.ToListAsync();
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(int id)
        {
            return await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<ProjectTask> CreateTaskAsync(ProjectTask task)
        {
            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<ProjectTask?> UpdateTaskAsync(int taskId, ProjectTask updatedTask)
        { 
            var existingTask = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (existingTask == null) return null;
            
            existingTask.Name = updatedTask.Name;
            existingTask.Description = updatedTask.Description;
            await _context.SaveChangesAsync();
            return existingTask;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return false;
            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
