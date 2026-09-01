using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }

        public int? CommentId { get; set; }
        public Comment? Comment { get; set; }

        public int? TaskAssignmentId { get; set; }
        public TaskAssignment? TaskAssignment { get; set; }
    }
}
