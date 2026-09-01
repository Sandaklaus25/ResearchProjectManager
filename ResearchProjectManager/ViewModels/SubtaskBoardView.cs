using System.Collections.Generic;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class SubtaskBoardViewModel
    {
        public TaskAssignment Assignment { get; set; } = null!;
        public int CourseId { get; set; }
        public string ThemeColor { get; set; }
        public bool IsStudent { get; set; }
    }
}