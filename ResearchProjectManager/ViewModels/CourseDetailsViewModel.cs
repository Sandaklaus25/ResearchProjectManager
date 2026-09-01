using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; } = null!;
        public int? StudentCount { get; set; }
        public int CurrentUserId { get; set; }
    }
}