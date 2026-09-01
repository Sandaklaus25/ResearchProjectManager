using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public int TaskAssignmentId { get; set; }
        public TaskAssignment TaskAssignment { get; set; }

        // The crucial structural change: Attachments live inside the Comment
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
