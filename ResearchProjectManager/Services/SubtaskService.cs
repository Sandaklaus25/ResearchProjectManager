using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.Services
{
    public class SubtaskService
    {
        private readonly ApplicationDbContext _context;

        public SubtaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Subtask> AddSubtaskAsync(Subtask subtask)
        {
            subtask.Status = Enums.TaskStatus.NotStarted;
            _context.Subtasks.Add(subtask);
            await _context.SaveChangesAsync();
            return subtask;
        }

        public async Task<Subtask> UpdateSubtaskAsync(Subtask subtask)
        {
            var existingSubtask = await _context.Subtasks.FindAsync(subtask.Id);
            if (existingSubtask == null)
            {
                throw new KeyNotFoundException($"Subtask with ID {subtask.Id} not found.");
            }
            existingSubtask.Name = subtask.Name;
            existingSubtask.Description = subtask.Description;
            existingSubtask.Status = subtask.Status;
            await _context.SaveChangesAsync();
            return existingSubtask;
        }

        public async Task DeleteSubtaskAsync(int id)
        {
            var subtask = await _context.Subtasks.FindAsync(id);
            if (subtask == null)
            {
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");
            }
            _context.Subtasks.Remove(subtask);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubtaskStatusAsync(int id, Enums.TaskStatus status)
        {
            var subtask = await _context.Subtasks.FindAsync(id);
            if (subtask == null)
            {
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");
            }
            subtask.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Subtask>> GetSubtasksByTaskAssignmentIdAsync(int taskAssignmentId)
        {
            return await _context.Subtasks.Where(s => s.TaskAssignmentId == taskAssignmentId).ToListAsync();
        }
    }
}
