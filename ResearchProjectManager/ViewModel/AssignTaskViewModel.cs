using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Add this!
using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class AssignTaskViewModel
    {
        public int ProjectTaskId { get; set; }
        public string TaskName { get; set; }

        // 1. Make it Nullable (?) and Required
        [Required(ErrorMessage = "You must set a deadline for this assignment.")]
        public DateTime? Deadline { get; set; }

        public List<int> SelectedStudentIds { get; set; } = new List<int>();
        public List<User> AvailableStudents { get; set; } = new List<User>();
    }
}