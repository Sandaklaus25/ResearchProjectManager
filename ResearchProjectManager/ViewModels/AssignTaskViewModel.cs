using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class AssignTaskViewModel
    {
        [Required(ErrorMessage = "Please select a task to assign.")]
        public int ProjectTaskId { get; set; }

        public int CourseId { get; set; }

        public string ThemeColor { get; set; } = "#0d6efd";
        public List<ProjectTask> AvailableTasks { get; set; } = new List<ProjectTask>();

        [Required(ErrorMessage = "A deadline is required.")]
        public DateTime? Deadline { get; set; }

        [Required]
        public string AssignmentMethod { get; set; } = "NewTeam"; 

        public List<int> SelectedStudentIds { get; set; } = new List<int>();
        public string? NewTeamName { get; set; }
        public List<User> AvailableStudents { get; set; } = new List<User>();

        public int? SelectedTeamId { get; set; }
        public List<Team> AvailableTeams { get; set; } = new List<Team>();
    }
}