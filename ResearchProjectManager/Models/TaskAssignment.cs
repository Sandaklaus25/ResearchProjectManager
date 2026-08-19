using System;
using System.Collections.Generic;
using ResearchProjectManager.Enums;

namespace ResearchProjectManager.Models
{
    public class TaskAssignment
    {
        public int Id { get; set; }
        public TaskStatusEnum TaskStatus { get; set; }
        public DateTime Deadline { get; set; }

        //Null if not completed yet, otherwise the date of completion
        public DateTime? CompletedAt { get; set; }
        public int TaskId { get; set; }
        public ProjectTask Task { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public List<Subtask> Subtasks { get; set; } = new List<Subtask>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
