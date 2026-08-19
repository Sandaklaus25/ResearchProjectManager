using Microsoft.AspNetCore.Identity;

namespace ResearchProjectManager.Models
{
    public class Role : IdentityRole<int>
    {
        public string Description { get; set; }

        public Role() { }

        public Role(string name, string description = null) : base(name)
        {
            Description = description;
        }

        // Predefined roles
        public static class PredefinedRoles
        {
            public const string Admin = "Admin";
            public const string Student = "Student";
            public const string Instructor = "Instructor";
            public const string Assistant = "Assistant";

           
            public static readonly Dictionary<string, string> AllRoles = new()
            {
                { Admin, "Administrator role with full system access" },
                { Student, "Student role with limited access" },
                { Instructor, "Instructor role for managing courses and students" },
                { Assistant, "Assistant role for supporting instructors" }
            };
        }
    }
}
