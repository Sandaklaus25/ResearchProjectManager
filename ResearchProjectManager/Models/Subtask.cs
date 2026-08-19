using ResearchProjectManager.Enums;

namespace ResearchProjectManager.Models
{
    public class Subtask
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskStatusEnum Status { get; set; }
        public int TaskAssignmentId { get; set; }
        public TaskAssignment TaskAssignment { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

    }
}
