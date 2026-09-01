using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(40)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CourseId { get; set; }

        public Course Course { get; set; }

        public int? AuthorId { get; set; }

        public User? Author { get; set; }


        public List<Subtask> BaseSubtasks { get; set; } = new List<Subtask>();
        public List<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
