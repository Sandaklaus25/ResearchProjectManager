using ResearchProjectManager.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.Models
{
    public class TaskAssignment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Name { get; set; }
        public Enums.TaskStatus TaskStatus { get; set; }
        public DateTime Deadline { get; set; }

        [Range(0, 100)]
        public int? Grade { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TaskId { get; set; }
        public ProjectTask Task { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public List<Subtask> Subtasks { get; set; } = new List<Subtask>();
        public List<Comment> Comments { get; set; } = new List<Comment>();

        public List<Attachment> TurnInAttachments { get; set; } = new List<Attachment>();
    }
}
