using System.Collections.Generic;
namespace ResearchProjectManager.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public List<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
        public List<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
