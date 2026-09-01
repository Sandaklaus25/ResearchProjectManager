using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using ResearchProjectManager.Enums;

namespace ResearchProjectManager.Models
{
    public class User : IdentityUser<int>
    {
        public List<CourseMember> EnrolledIn { get; set; } = new List<CourseMember>();
        public List<Subtask> Subtasks { get; set; } = new List<Subtask>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
    }
}
