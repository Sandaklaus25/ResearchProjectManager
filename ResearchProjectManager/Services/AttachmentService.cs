using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.Services
{
    public class AttachmentService
    {
        private readonly ApplicationDbContext _context;

        public AttachmentService(ApplicationDbContext context)
        {
            _context = context; 
        }

        public async Task<Attachment> AddAttachmentRecordAsync(Attachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<List<Attachment>> GetAttachmentsForAssigmentAsync(int taskAssignmentId)
        {
            return await _context.Attachments
                .Include(a => a.User)
                .Where(a => a.TaskAssignmentId == taskAssignmentId)
                .ToListAsync();
        }

        public async Task DeleteAttachmentRecordAsync(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment == null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
