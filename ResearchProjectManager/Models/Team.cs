using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ResearchProjectManager.Models
{
    public class Team
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string? Name { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public List<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
        public List<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
