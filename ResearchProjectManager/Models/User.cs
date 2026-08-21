using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using ResearchProjectManager.Enums;

namespace ResearchProjectManager.Models
{
    public class User : IdentityUser<int>
    {
        public List<CourseMembers> EnrolledIn { get; set; } = new List<CourseMembers>();
        public List<Subtask> Subtasks { get; set; } = new List<Subtask>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public List<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
    }
}
