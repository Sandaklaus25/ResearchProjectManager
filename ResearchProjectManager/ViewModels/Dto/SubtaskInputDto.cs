using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.ViewModels.Dto
{
    public class SubtaskInputDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
