using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.Services
{
    public class CommentService
    {
        private readonly ApplicationDbContext _context;
        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }
        public void AddComment(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }
        public async Task<List<Comment>> GetCommentsByTaskAssignmentIdAsync(int taskAssignmentId)
        {
            return await _context.Comments
                .Where(c => c.TaskAssignmentId == taskAssignmentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

    }
}
