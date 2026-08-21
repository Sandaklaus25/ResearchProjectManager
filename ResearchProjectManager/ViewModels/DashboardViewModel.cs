using System.Collections.Generic;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.ViewModels
{
    public class DashboardViewModel
    {
        // For Admins/Instructors to see all high-level projects
        public List<ProjectTask> AllProjects { get; set; } = new List<ProjectTask>();

        // For Student Teams to see their specific active workloads
        public List<TaskAssignment> ActiveAssignments { get; set; } = new List<TaskAssignment>();
    }
}