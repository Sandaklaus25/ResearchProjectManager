using System;
using System.Collections.Generic;

namespace ResearchProjectManager.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public int AuthorId { get; set; }

        public User Author { get; set; }

        public List<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
