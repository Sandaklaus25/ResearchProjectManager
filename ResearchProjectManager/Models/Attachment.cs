namespace ResearchProjectManager.Models
{
    public class Attachment
    {
        // Do later Base64 encode the file and store it in cloud or smth
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TaskAssignmentId { get; set; }
        public TaskAssignment TaskAssignment { get; set; }
    }
}
