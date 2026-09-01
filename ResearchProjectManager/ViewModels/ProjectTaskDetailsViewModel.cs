using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class ProjectTaskDetailsViewModel
    {
        public ProjectTask Task { get; set; } = null;
        public int CourseId { get; set; }
        public string ThemeColor { get; set; }
    }
}