using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.Models
{
    public class Course
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsPrivate { get; set; }

        [Required]
        [StringLength(7)] // Limits the database column to 7 characters (e.g., "#FF5733")
        [RegularExpression("^#([A-Fa-f0-9]{6})$")]
        public string Color { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        public List<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

        public List<CourseMembers> Members { get; set; } = new List<CourseMembers>();


    }
}
