using ResearchProjectManager.Models;
using System.Collections.Generic;

namespace ResearchProjectManager.ViewModels
{
    public class CourseMemberListViewModel
    {
        public int CourseId { get; set; }
        public string ThemeColor { get; set; }
        public int CurrentUserId { get; set; }
        public bool IsCurrentUserOwner { get; set; }

        public int OwnerId { get; set; }
        public string CurrentUserRole { get; set; }

        public List<User> Instructors { get; set; } = new();
        public List<User> Assistants { get; set; } = new();
        public List<User> Students { get; set; } = new();
        public List<User> BannedUsers { get; set; } = new();
    }
}