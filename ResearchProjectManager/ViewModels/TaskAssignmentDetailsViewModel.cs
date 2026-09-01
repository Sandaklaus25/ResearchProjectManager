using System.Collections.Generic;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class TaskAssignmentDetailsViewModel
    {
        public TaskAssignment Assignment { get; set; } = null!;
        public int CourseId { get; set; }
        public string ThemeColor { get; set; }
        public string BackAction { get; set; }
        public bool IsStudent { get; set; }
        public bool IsInstructorOrTA { get; set; }
        public bool AllStepsFinished { get; set; }
    }
}