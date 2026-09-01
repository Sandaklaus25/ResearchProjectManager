using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class CoursesViewModel
    {
        public List<Course> AllCourses { get; set; } = new List<Course>();
        public Dictionary<int, int> StudentCounts { get; set; } = new Dictionary<int, int>();
    }
}
