using System.Collections.Generic;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class ProjectTaskListViewModel
    {
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public int CourseId { get; set; }
        public string ThemeColor { get; set; }
    }
}