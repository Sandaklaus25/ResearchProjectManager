namespace ResearchProjectManager.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CommentText { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public int UserId { get; set; }

        public User User { get; set; }

        public int TaskAssignmentId { get; set; }
        public TaskAssignment TaskAssignment { get; set; }
    }
}
