using ResearchProjectManager.Enums;
using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9]*$")]
        [StringLength(9, MinimumLength = 9)]
        public string SpecialCode { get; set; } // Unique code

        public bool IsPrivate { get; set; }

        [Required]
        public required CourseColor Color { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        public List<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

        public List<CourseMember> Members { get; set; } = new List<CourseMember>();


    }
}
