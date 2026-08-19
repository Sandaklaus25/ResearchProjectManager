using System;
using System.Collections.Generic;

namespace ResearchProjectManager.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public List<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
