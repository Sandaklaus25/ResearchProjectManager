using ResearchProjectManager.Enums;
using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.Models
{
    public class Subtask
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)] 
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }
        public Enums.TaskStatus Status { get; set; }

        // --- NEW TRACKING PROPERTIES ---
        public bool IsBaseTask { get; set; } = false;

        public int? CreatorId { get; set; }
        public User? Creator { get; set; }

        public int? LastStatusUpdaterId { get; set; }
        public User? LastStatusUpdater { get; set; }
        // -------------------------------

        public int? TaskAssignmentId { get; set; }
        public TaskAssignment? TaskAssignment { get; set; }

        public int? ProjectTaskId { get; set; }
        public ProjectTask? ProjectTask { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}