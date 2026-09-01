using System.Collections.Generic;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class TaskAssignmentListViewModel
    {
        public List<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public int CourseId { get; set; }
        public string ThemeColor { get; set; }
    }
}
